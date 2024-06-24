using Fenrir.Multiplayer;
using Fenrir.Multiplayer.Rooms;
using System.Collections.Generic;
using System;

namespace Fenrir.ECS
{
    public abstract class SimulationServerRoom<TInput> : ServerRoom
        where TInput : struct, IByteStreamSerializable
    {
        /// <summary>
        /// Simulation
        /// </summary>
        private Simulation _simulation;

        /// <summary>
        /// Simulation input buffer
        /// </summary>
        private InputBuffer<TInput> _inputBuffer;

        /// <summary>
        /// Server clock
        /// </summary>
        private Clock _clock;

        /// <summary>
        /// Queued requests per player
        /// </summary>
        private Dictionary<int, Queue<PlayerInputRequest<TInput>>> _queuedInputs = new Dictionary<int, Queue<PlayerInputRequest<TInput>>>();

        /// <summary>
        /// Simulation players
        /// </summary>
        private PlayerReference[] _players;

        /// <summary>
        /// Current inputs
        /// </summary>
        private InputBuffer<TInput> _currentInputs = new InputBuffer<TInput>();

        /// <summary>
        /// Previous inputs
        /// </summary>
        private InputBuffer<TInput> _previousInputs = new InputBuffer<TInput>();

        /// <summary>
        /// Simulation start time
        /// </summary>
        private DateTime _simulationStartTime;

        /// <summary>
        /// True if simulation has started
        /// </summary>
        private volatile bool _isRunning = false;

        /// <summary>
        /// Simulation time
        /// </summary>
        private DateTime SimulationTime => _clock.UtcNow;

        /// <summary>
        /// Simulation clock
        /// </summary>
        public Clock Clock => _clock;

        /// <summary>
        /// Simulation
        /// </summary>
        public Simulation Simulation => _simulation;

        /// <summary>
        /// Simulation input buffer
        /// </summary>
        public InputBuffer<TInput> InputBuffer => _inputBuffer;


        public SimulationServerRoom(ILogger logger, string roomId, Clock clock)
            : base(logger, roomId)
        {
            // Constructor...
            _clock = clock;
            _simulation = new Simulation(clock, logger);
            _inputBuffer = new InputBuffer<TInput>();
        }

        protected override RoomJoinResponse OnBeforePeerJoin(IServerPeer peer, string token)
        {
            // This method allows to validate peer before they join

            if (peer.PeerData != null)
            {
                return new RoomJoinResponse(false, 1, "Already in a room");
            }

            return RoomJoinResponse.JoinSuccess;
        }

        protected void IngestInput(PlayerReference player, PlayerInputRequest<TInput> request)
        {
            Queue<PlayerInputRequest<TInput>> queuedPlayerInputs;

            if (_queuedInputs.ContainsKey(player.PlayerId))
            {
                queuedPlayerInputs = _queuedInputs[player.PlayerId];
            }
            else
            {
                queuedPlayerInputs = new Queue<PlayerInputRequest<TInput>>();
                _queuedInputs[player.PlayerId] = queuedPlayerInputs;
            }

            queuedPlayerInputs.Enqueue(request);
        }

        protected void RunSimulation(PlayerReference[] players)
        {
            // Start time
            _simulationStartTime = _clock.UtcNow;

            // Players
            _players = players;

            // Set is running
            _isRunning = true;

            // Run simulation
            TickSimulation();
        }

        protected void StopSimulation()
        {
            _isRunning = false;
        }

        private void TickSimulation()
        {
            if (!_isRunning)
                return; // Stopped

            try
            {
                Tick();
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
                Logger.Error("Uncaught exception during simulation Tick()");
            }

            // Schedule next tick
            DateTime currentTickTime = _simulationStartTime + _simulation.CurrentTickTime;
            TimeSpan currentTickIn = currentTickTime - SimulationTime;

            //Logger.Info($"Waiting for tick {_simulation.CurrentTick} at {SimulationTime.ToString("h:m:s.ffff")}. Current tick is at {nextTickTime.ToString("h:m:s.ffff")}, waiting for {nextTickIn} until current tick");

            Schedule(TickSimulation, currentTickIn > TimeSpan.Zero ? currentTickIn : TimeSpan.Zero);
        }

        protected virtual void OnTick()
        {
        }

        private void Tick()
        {
            // Create new snapshot
            _simulation.CaptureSnapshot();

            //Console.WriteLine($"[Server at Tick {_simulation.CurrentTick}] Tick simulation at {_clock.UtcNow.ToString("h:m:s.ffff")}, " +
            //    $"next pending input for client 0 is for tick {(_queuedInputs[0].Count > 0 ? _queuedInputs[0].Peek().NumTick : -1)}, number of queued inputs is {_queuedInputs[0].Count} " +
            //    $"next pending input for client 1 is for tick {(_queuedInputs[1].Count > 0 ? _queuedInputs[1].Peek().NumTick : -1)}, number of queued inputs is {_queuedInputs[1].Count} ");

            // Apply input for the current tick
            for (int i = 0; i < _players.Length; i++)
            {
                if(_queuedInputs.TryGetValue(i, out Queue<PlayerInputRequest<TInput>> queuedInputs))
                {
                    PlayerReference player = _players[i];
                    while (queuedInputs.TryPeek(out PlayerInputRequest<TInput> playerInputRequest))
                    {
                        if (playerInputRequest.NumTick > _simulation.CurrentTick)
                        {
                            // Wait for the correct tick to dispatch this input
                            break;
                        }
                        else // playerInputRequest.NumTick == _simulation.CurrentTick OR arrived too late. If tick arrived too late apply it anyway and client will have to roll back?
                        {
                            // Time to apply this input (same tick)
                            queuedInputs.Dequeue();
                            _inputBuffer.SetInput(player.PlayerId, playerInputRequest.Input);
                            _previousInputs.CopyFrom(_currentInputs);
                            _currentInputs.SetInput(i, playerInputRequest.Input);
                        }
                    }
                }
            }

            // Tick systems
            _simulation.TickSystems();

            // Commit added/remove entities etc
            _simulation.Commit();

            // Broadcast tick event
            // Logger.Info($"Broadcasting tick data for tick {_simulation.CurrentTick} time is {_clock.UtcNow.ToString("h:m:s.ffff")}. Player1 input={_currentInputs[0]} player2 input={_currentInputs[1]}");
            
            // TODO We need to find more optimized way to do this. Perhaps we could directly pass in the InputBuffer into the simulation tick event?
            TInput[] playerInputs = new TInput[_players.Length];

            for (int i = 0; i < _players.Length; i++)
            {
                PlayerReference player = _players[i];

                if(_currentInputs.TryGetInput(player.PlayerId, out TInput input))
                {
                    playerInputs[i] = input;
                }
                else if(_previousInputs.TryGetInput(player.PlayerId, out TInput previousInput))
                {
                    // Input dropped from this player, predict
                    playerInputs[i] = PredictInput(input);
                }
                else
                { 
                    // No input for this player, assume empty
                    playerInputs[i] = new TInput();
                }
            }

            BroadcastEvent(new SimulationTickEvent<TInput>(_simulation.CurrentTick, playerInputs));

            // Increment tick
            _simulation.CurrentTick++;

            // Invoke OnTick handler
            OnTick();
        }

        protected virtual TInput PredictInput(TInput previousInput)
        {
            return previousInput;
        }
    }
}
