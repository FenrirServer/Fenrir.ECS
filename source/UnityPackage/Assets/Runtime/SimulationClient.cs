using Fenrir.Multiplayer;
using Fenrir.Multiplayer.Rooms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Fenrir.ECS
{
    public class SimulationClient<TInput>
        : ISimulationClient
        , IEventHandler<ClockSyncAckEvent>
        , IEventHandler<SimulationTickEvent<TInput>>
        , IDisposable
        where TInput : struct, IByteStreamSerializable
    {
        private readonly NetworkClient _networkClient;
        private readonly ILogger _logger;
        private readonly Clock _clock;
        private readonly ClockSynchronizer _clockSynchronizer;
        private readonly Simulation _simulation;
        private readonly InputBuffer<TInput> _inputBuffer;

        /// <summary>
        /// Player representing this client
        /// </summary>
        public PlayerReference ThisPlayer => _thisPlayer;

        /// <summary>
        /// Simulation
        /// </summary>
        public Simulation Simulation => _simulation;

        /// <summary>
        /// Simulation inputs
        /// </summary>
        public InputBuffer<TInput> InputBuffer => _inputBuffer;

        /// <summary>
        /// Network Client
        /// </summary>
        public NetworkClient NetworkClient => _networkClient;

        /// <summary>
        /// Clock Synchronizer
        /// </summary>
        public ClockSynchronizer ClockSynchronizer => _clockSynchronizer;

        /// <summary>
        /// Last confirmed simulation tick #
        /// </summary>
        public int LastConfirmedTick => _lastConfirmedTick;

        /// <summary>
        /// Current player input
        /// </summary>
        private TInput _currentPlayerInput;

        /// <summary>
        /// Last confirmed tick number
        /// </summary>
        private int _lastConfirmedTick = -1;

        /// <summary>
        /// Time when last tick was confirmed
        /// </summary>
        private DateTime _lastConfirmedTickTime;

        /// <summary>
        /// Buffer that holds last confirmed inputs
        /// </summary>
        private TInput[] _lastConfirmedInputs;

        /// <summary>
        /// Unconfirmed/pending frame inputs
        /// When we receive simulation tick event from the server, 
        /// we confirm and dequeue the pending input from here, OR rollback
        /// </summary>
        private Queue<TInput> _unconfirmedPlayerInputs = new Queue<TInput>();

        /// <summary>
        /// Queue that holds pending authority ticks (that will either confirm a prediction or trigger a roll back)
        /// </summary>
        private Queue<SimulationTickEvent<TInput>> _incomingAuthorityTicks = new Queue<SimulationTickEvent<TInput>>();

        /// <summary>
        /// Initial clock synchronization timeout
        /// </summary>
        public TimeSpan InitialClockSyncTimeout { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Number of initial clock synchronization events, 
        /// that have to be received in order to start a simulation
        /// </summary>
        public int NumInitialClockSyncEvents { get; set; } = 5;

        /// <summary>
        /// Delay between initial clock sync requests.
        /// This is a magically selected number, based on the assumption that avg player
        /// will have a ping somewhere between 10ms and 100ms.
        /// It will take 50ms to send all pings out, and we are very likely to start receiving some acks by then
        /// </summary>
        public double InitialClockSyncDelayMs { get; set; } = 100;

        /// <summary>
        /// Difference between the client and server time, allowing client simulation to run ahead of the server
        /// and send inputs that will arrive just in time for server to ingest for the upcoming tick.
        /// Currently calculated as a half RTT + a buffer (several ticks)
        /// </summary>
        private TimeSpan TargetSimulationTimeOffset => new TimeSpan(_clockSynchronizer.AvgRoundTripTime.Ticks / 2) + _simulation.TimePerTick;

        /// <summary>
        /// Average client round trip time
        /// </summary>
        public TimeSpan RoundTripTime => _clockSynchronizer.AvgRoundTripTime;

        /// <summary>
        /// Average client round trip time
        /// </summary>
        public TimeSpan RoundTripTimeOver2 => new TimeSpan(_clockSynchronizer.AvgRoundTripTime.Ticks / 2);

        /// <summary>
        /// Server Simulation time
        /// </summary>
        public DateTime ServerTime => _clock.UtcNow;

        /// <summary>
        /// Simulation time. Calculated as server time plus local simulation offset
        /// Local simulation always runs ahead of the server simulation time, to allow
        /// clients to send inputs that will arrive just in time for the server to process them
        /// for the upcoming tick.
        /// </summary>
        public DateTime SimulationTime => ServerTime + _currentSimulationTimeOffset;

        /// <summary>
        /// Simulation debug info
        /// </summary>
        public DebugInfo DebugInfo => GetDebugInfo();

        /// <summary>
        /// Threshold after which we should just snap the offset instead of applying smooth correction
        /// </summary>
        public TimeSpan SmoothClockOffsetThreshold { get; set; } = TimeSpan.FromMilliseconds(200);

        /// <summary>
        /// Smooth clock correction step size
        /// </summary>
        public TimeSpan SmoothClockOffsetCorrectionStepSize { get; set; } = TimeSpan.FromMilliseconds(1);

        /// <summary>
        /// Set to predict inputs. If set to null, next input is predicted by simply repeating the last input.
        /// This will lead to more mispredictions and rollbacks.
        /// If your input has any transient flags, you should set this handler.
        /// </summary>
        public Func<TInput, TInput> PredictInputHandler { get; set; } = null;

        /// <summary>
        /// Current simulation offset
        /// </summary>
        private TimeSpan _currentSimulationTimeOffset;

        /// <summary>
        /// Simulation start time
        /// </summary>
        private DateTime _simulationStartTime;

        /// <summary>
        /// Clock synchronization task completion source.
        /// Completes when initial clock synchronization is done
        /// </summary>
        private TaskCompletionSource<bool> _clockSyncInitTcs = null;

        /// <summary>
        /// Simulation observers
        /// </summary>
        private List<ISimulationObserver> _simulationObservers = new List<ISimulationObserver>();

        /// <summary>
        /// Simulation observers
        /// </summary>
        private List<EntityViewObserver> _entityViewObservers = new List<EntityViewObserver>();

        /// <summary>
        /// List of all players in the simulation
        /// </summary>
        private List<PlayerReference> _players = null;

        /// <summary>
        /// A player controlled by this client
        /// </summary>
        private PlayerReference _thisPlayer;

        /// <summary>
        /// Indicates if simulation is running
        /// </summary>
        private volatile bool _isRunning = false;

        /// <summary>
        /// Sync root
        /// </summary>
        private object _syncRoot = new object();

        public SimulationClient(ILogger logger)
        {
            _networkClient = new NetworkClient(logger);
            _logger = logger;
            _clock = new Clock();
            _clockSynchronizer = new ClockSynchronizer();
            _simulation = new Simulation(_clock, _logger);
            _inputBuffer = new InputBuffer<TInput>();

            // Subscribe to network events
            _networkClient.Disconnected += OnDisconnected;

            // Add event listeners
            _networkClient.AddEventHandler<ClockSyncAckEvent>(this);
            _networkClient.AddEventHandler<SimulationTickEvent<TInput>>(this);
        }

        #region Lifecycle
        public async Task Connect(Uri serverUri, string roomId, string joinToken)
        {
            ConnectionResponse connectionResponse = await _networkClient.Connect(serverUri);
            if (!connectionResponse.Success)
            {
                _logger.Error("Connection failed: " + connectionResponse.Reason);
                return;
            }

            // Sync server clock
            await SyncSimulationClock().TimeoutAfter(InitialClockSyncTimeout);

            // Join room
            RoomJoinResponse joinRoomResponse = await _networkClient.JoinRoom(roomId, joinToken);
            if (!joinRoomResponse.Success)
            {
                _logger.Error("Join room failed: " + joinRoomResponse.Reason);
                return;
            }
        }

        public void Disconnect()
        {
            _networkClient?.Disconnect();
        }

        private void OnDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            _logger.Info("Disconnected from server");
            Stop();
        }
        #endregion

        #region Input Management
        public void ApplyInput(TInput playerInput)
        {
            lock (_syncRoot)
            {
                _currentPlayerInput = playerInput;
            }
        }

        #endregion

        #region Event Handlers
        void IEventHandler<ClockSyncAckEvent>.OnReceiveEvent(ClockSyncAckEvent evt)
        {
            //_logger.Info($"Received clock sync ack event, current clock offset is {_clock.Offset.TotalMilliseconds} ms, avg round trip time is {_clockSynchronizer.AvgRoundTripTime.TotalMilliseconds} ms, avg offset is {_clockSynchronizer.AvgOffset.TotalMilliseconds} ms, next recommended sync time is { _clockSynchronizer.NextSyncTime}");

            lock (_syncRoot)
            {
                DateTime timeReceivedResponse = _clock.UtcNowRaw;

                // Record clock synchronization data
                _clockSynchronizer.RecordSyncResult(evt.TimeSentRequest, evt.TimeReceivedRequest, evt.TimeSentResponse, timeReceivedResponse);

                // Check if this is an initial sync, before simulation has started
                bool initialSync = _clockSyncInitTcs != null;

                // Update offset
                UpdateTargetClockOffset(initialSync);

                // Check if we need to complete initial clock synchronization
                if (initialSync && _clockSynchronizer.NumRoundTripsRecorded >= NumInitialClockSyncEvents)
                {
                    var tcs = _clockSyncInitTcs;
                    _clockSyncInitTcs = null;
                    tcs.SetResult(true);
                }
            }
        }
        void IEventHandler<SimulationTickEvent<TInput>>.OnReceiveEvent(SimulationTickEvent<TInput> evt)
        {
            //_logger.Info($"Incoming tick event for tick {evt.NumTick}, time={SimulationTime.ToString("h:m:s.ffff")} current tick is {_simulation.CurrentTick}");

            lock (_syncRoot)
            {
                _incomingAuthorityTicks.Enqueue(evt);
            }
        }

        #endregion

        #region Simulation
        public void Start(SimulationState initialState)
        {
            _players = initialState.Players;
            _thisPlayer = _players.Where(player => _networkClient.Peer.Id == player.PeerId.ToString()).First();

            _isRunning = true;

            Task.Run(() => RunTickThread(initialState));
        }

        public void Stop()
        {
            _isRunning = false;
            _clockSyncInitTcs = null;
        }

        private async Task RunTickThread(SimulationState initialState)
        {
            // Manually poll network client events before this tick
            _networkClient.PollEvents();

            // Set initial simulation state
            _simulation.CurrentTick = initialState.CurrentTick;

            // Set the initial tick data
            _simulation.ECSWorld.SetTickData(initialState.CurrentTickData);

            // Calculate time of the first simulation tick
            DateTime firstTickTime = initialState.CurrentTickTime - Simulation.TimePerTick * initialState.CurrentTick;

            // Run ticks
            while (_isRunning && _networkClient.State == ConnectionState.Connected)
            {
                // Wait for the current tick
                DateTime currentTickTime = firstTickTime + _simulation.CurrentTickTime; // first tick time +  current tick * ms per tick
                TimeSpan currentTickIn = currentTickTime - SimulationTime;
                await Task.Delay(currentTickIn > TimeSpan.Zero ? currentTickIn : TimeSpan.Zero);

                // Tick
                lock (_syncRoot)
                {
                    try
                    {
                        Tick();
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e.ToString());
                        _logger.Error("Uncaught exception during simulation Tick()");
                    }
                }
            }
        }

        private void TickSimulation(TInput[] playerInputs, bool didRollBack = false, bool confirmedTick = false)
        {
            // Copy previous snapshot data
            _simulation.CaptureSnapshot();

            // Increment to the next tick
            _simulation.CurrentTick++;

            // Apply inputs
            for (int numPlayer = 0; numPlayer < playerInputs.Length; numPlayer++)
            {
                PlayerReference player = _players[numPlayer];
                TInput playerInput = playerInputs[numPlayer];
                _inputBuffer.SetInput(player.PlayerId, playerInput);
            }

            // Tick systems
            _simulation.TickSystems();

            // Commit
            var commitResult = _simulation.Commit();

            // Invoke tick observers
            InvokeSimulationTickObservers(commitResult, didRollBack);

            if (confirmedTick)
            {
                ConfirmTick(_simulation.CurrentTick, _simulation.GetCurrentTickData(), commitResult);
            }
        }

        private void Tick()
        {
            // Send current player input
            SendCurrentPlayerInput();

            // Confirm previous ticks
            ApplyAuthorityTicks();

            // Add to the list of unconfirmed player inputs
            _unconfirmedPlayerInputs.Enqueue(_currentPlayerInput);

            TInput[] playerInputs;

            // Special case: check if next (first) authority tick matches this simulation tick #. In this case simply use it right now, instead of having to roll back
            bool confirmedTick = false;
            if (_incomingAuthorityTicks.TryPeek(out SimulationTickEvent<TInput> earliestConfirmedTickEvent)
                && earliestConfirmedTickEvent.NumTick == _simulation.CurrentTick)
            {
                _incomingAuthorityTicks.Dequeue(); // Next confirmed tick matches this tick, remove and use it's inputs
                _unconfirmedPlayerInputs.Dequeue();
                _lastConfirmedInputs = earliestConfirmedTickEvent.Inputs;
                playerInputs = _lastConfirmedInputs;
                _lastConfirmedTick = _simulation.CurrentTick;
                _lastConfirmedTickTime = SimulationTime;
                confirmedTick = true;
            }
            else
            {
                // Predict inputs
                playerInputs = PredictNextInputs(_currentPlayerInput); // same as _unconfirmedPlayerInputs.Last()
            }

            //_logger.Info($"OPponent input velocity is {opponentInput.Velocity} rotation {opponentInput.RotationRads}");

            // Tick simulation
            TickSimulation(playerInputs, false, confirmedTick);

            // Check if we need to send clock sync request at all
            if (_clock.UtcNowRaw > _clockSynchronizer.NextSyncTime)
            {
                SendSyncClockRequest();
            }

            // Apply clock correction
            ApplySimulationClockSmoothCorrection();
        }

        private bool DidMispredictInputs(TInput[] authorityInputs, TInput thisPlayerInputAtTick)
        {
            for (int i = 0; i < authorityInputs.Length; i++)
            {
                TInput authorityInput = authorityInputs[i];

                if(i == _thisPlayer.PlayerId)
                {
                    // For this player, check against a given input at tick
                    // If this input at tick does not match authority input, it means their input was dismissed (either player was hacked or lagged beyond lag compensation and authority already ticked without their input)
                    if (!authorityInput.Equals(thisPlayerInputAtTick))
                        return true;
                }
                else
                {
                    // For other players, check against last confirmed inputs (previous authority ticks)
                    // Since most input prediction here is simply copy certain fields,
                    // Instead of tracking each predicted input, simply re-predict here and check:
                    // Does authority input match with what we have predicted?

                    // This does NOT work if the client chooses to actually run some prediction logic (e.g. extrapolate rotation etc)
                    // instead of simply predicting that the button is still pressed.
                    // In that case we need to refactor and track all predicted inputs not just player one
                    TInput lastConfirmedOtherPlayerInput = _lastConfirmedInputs != null ? _lastConfirmedInputs[i] : new TInput();
                    TInput predictedInput = PredictNextInputForOtherPlayer(lastConfirmedOtherPlayerInput);
                    if (!authorityInput.Equals(predictedInput))
                        return true;
                }
            }

            return false;
        }

        private void ApplyAuthorityTicks()
        {
            // E.g. the client is at tick # 105
            // Normally the queue may look like: ->[98, 97,96]-> e.g. since our last tick
            // the server confirmed ticks 96,97 and 98
            // In this case, roll the simulation back until tick 97,
            // And re-run the simulation, applying confirmed inputs for ticks 96,97,98 and predicting for 99-105

            int currentTick = _simulation.CurrentTick;

            // Check if and how far we need to roll back.
            // Check how many server ticks we have with tick # smaller than the current tick.
            // We will roll back by that number of ticks and use them to re-simulate and apply authority input.
            int numTicksToRollBack = 0;
            while (_incomingAuthorityTicks.TryPeek(out SimulationTickEvent<TInput> authorityTickEvent) 
                && authorityTickEvent.NumTick < currentTick)
            {
                // Check if this tick is confirmed (matches predicted), or need to be rolled back.
                TInput unconfirmedInput = _unconfirmedPlayerInputs.Peek();

                if(DidMispredictInputs(authorityTickEvent.Inputs, unconfirmedInput))
                {
                    //if(thisPlayerInput != unconfirmedInput)
                    //{
                    //    _logger.Warning("INVALIDATED INPUT FOR THIS PLAYER");
                    //}

                    // This tick is invalid, re-simulate starting from it
                    numTicksToRollBack = currentTick - authorityTickEvent.NumTick;
                    break;
                }
                else
                {
                    // This tick is valid, remove both authority tick and player input and check the next one
                    _incomingAuthorityTicks.Dequeue();
                    _unconfirmedPlayerInputs.Dequeue();
                    _lastConfirmedTick = authorityTickEvent.NumTick;
                    _lastConfirmedTickTime = SimulationTime;
                    _lastConfirmedInputs = authorityTickEvent.Inputs;

                    // Confirm this tick, as it is a valid tick
                    int confirmedTickOffset = currentTick - authorityTickEvent.NumTick;
                    ConfirmTick(authorityTickEvent.NumTick, _simulation.GetTickData(confirmedTickOffset), _simulation.GetTickCommitResult(confirmedTickOffset));
                }
            }

            // If we need to roll back
            if(numTicksToRollBack > 0)
            {
                // Roll back by this number of ticks and re-apply inputs
                _simulation.Rollback(numTicksToRollBack);
                InvokeSimulationRollbackObservers(numTicksToRollBack);

                // Re-simulate and apply authority ticks, until we run out of authority ticks.
                // After that, re-simulate until current tick with predictions 
                while(_incomingAuthorityTicks.TryDequeue(out SimulationTickEvent<TInput> authorityTickEvent) 
                    && _simulation.CurrentTick <= currentTick)
                {
                    // Remove unconfirmed input as it will not be needed anymore (we are overwriting this with authority input)
                    _unconfirmedPlayerInputs.Dequeue();

                    // Tick simulation with authority inputs
                    TickSimulation(authorityTickEvent.Inputs, true, true);

                    _lastConfirmedTick = authorityTickEvent.NumTick;
                    _lastConfirmedTickTime = SimulationTime;
                    _lastConfirmedInputs = authorityTickEvent.Inputs;
                }

                // If we ran out of authority ticks, re-simulate until current tick predicting inputs
                int i = 0;
                while(_simulation.CurrentTick < currentTick)
                {
                    TInput unconfirmedThisPlayerInput = _unconfirmedPlayerInputs.ElementAt(i); // TODO switch to linked list implemented queue here instead of using Linq
                    TInput[] predictedInputs = PredictNextInputs(unconfirmedThisPlayerInput);
                    TickSimulation(predictedInputs);
                    i++;
                }
            }
        }

        private TInput[] PredictNextInputs(TInput thisPlayerPrevInput)
        {
            // TODO: Replace this garbage with a re-usable buffer

            TInput[] predictedInputs = new TInput[_players.Count];
            for (int numPlayer = 0; numPlayer < _players.Count; numPlayer++)
            {
                PlayerReference player = _players[numPlayer];
                if (player.PlayerId == _thisPlayer.PlayerId)
                {
                    predictedInputs[numPlayer] = thisPlayerPrevInput;
                }
                else
                {
                    TInput lastConfirmedOtherPlayerInput = _lastConfirmedInputs != null ? _lastConfirmedInputs[numPlayer] : new TInput(); // special case, for first N frames we might just predict empty input
                    predictedInputs[numPlayer] = PredictNextInputForOtherPlayer(lastConfirmedOtherPlayerInput);
                }
            }
            return predictedInputs;
        }

        private TInput PredictNextInputForOtherPlayer(TInput previousInput)
        {
            if(PredictInputHandler != null)
            {
                return PredictInputHandler(previousInput);
            }
            else
            {
                return previousInput;
            }
        }

        private void SendCurrentPlayerInput()
        {
            //_logger.Info($"[Client {_thisPlayer.PlayerId} at Tick {_simulation.CurrentTick}] Sending player input for player {_thisPlayer.PlayerId} at {_clock.UtcNow.ToString("h:m:s.ffff")} (clock offset {_clock.Offset}, average rtt is {_clockSynchronizer.AvgRoundTripTime}, expecting to arrive at {(ServerTime + _clockSynchronizer.AvgRoundTripTime/2).ToString("h:m:s.ffff")})");

            var playerInputRequest = new PlayerInputRequest<TInput>(_simulation.CurrentTick, _currentPlayerInput);
            _networkClient.Peer?.SendRequest<PlayerInputRequest<TInput>>(playerInputRequest, 0, MessageDeliveryMethod.ReliableOrdered);
        }

        private void ConfirmTick(int numTick, ArchetypeCollection tickData, CommitResult commitResult)
        {
            // Free snapshot from the world
            _simulation.FreeSnapshot();

            // Invoke confirm tick observers
            InvokeSimulationConfirmedTickObservers(numTick, tickData, commitResult);
        }

        #endregion

        #region Clock Sync
        private async Task SyncSimulationClock()
        {
            _clockSyncInitTcs = new TaskCompletionSource<bool>();

            // Keep sending those until initial synchronization is done
            while (_clockSyncInitTcs != null)
            {
                SendSyncClockRequest();

                if (_clockSyncInitTcs != null)
                {
                    await Task.WhenAny(_clockSyncInitTcs.Task, Task.Delay(TimeSpan.FromMilliseconds(InitialClockSyncDelayMs)));
                }
            }
        }

        private void SendSyncClockRequest()
        {
            var simClockSyncRequest = new ClockSyncRequest(_clock.UtcNowRaw);
            _networkClient.Peer?.SendRequest<ClockSyncRequest>(simClockSyncRequest, 0, MessageDeliveryMethod.Unreliable);
        }

        private void UpdateTargetClockOffset(bool immediate)
        {
            TimeSpan delta = TargetSimulationTimeOffset - _clock.Offset;

            if (immediate || Math.Abs(delta.TotalMilliseconds) > SmoothClockOffsetThreshold.TotalMilliseconds)
            {
                // Do not wait for smooth correction, apply right away
                _clock.Offset = _clockSynchronizer.AvgOffset;
                _currentSimulationTimeOffset = TargetSimulationTimeOffset; // Also update sim time update since it depends on the clock offset
            }
        }

        private void ApplySimulationClockSmoothCorrection()
        {
            // Apply clock offset from clock synchronizer
            if(_clock.Offset != _clockSynchronizer.AvgOffset)
            {
                TimeSpan delta = _clockSynchronizer.AvgOffset - _clock.Offset;
                double stepMs = Math.Max(SmoothClockOffsetCorrectionStepSize.TotalMilliseconds, Math.Abs(delta.TotalMilliseconds));
                _clock.Offset = TimeSpanExtensions.MoveTowards(_clock.Offset, _clockSynchronizer.AvgOffset, TimeSpan.FromMilliseconds(stepMs));
            }

            // Apply target simulation offset (changes when rtt changes)
            if (_currentSimulationTimeOffset != TargetSimulationTimeOffset)
            {
                TimeSpan delta = TargetSimulationTimeOffset - _currentSimulationTimeOffset;
                double stepMs = Math.Max(SmoothClockOffsetCorrectionStepSize.TotalMilliseconds, Math.Abs(delta.TotalMilliseconds));
                _currentSimulationTimeOffset = TimeSpanExtensions.MoveTowards(_currentSimulationTimeOffset, TargetSimulationTimeOffset, TimeSpan.FromMilliseconds(stepMs));
            }
        }
        #endregion

        #region Simulation Observer
        public void AddSimulationObserver(ISimulationObserver simulationObserver)
        {
            _simulationObservers.Add(simulationObserver);
        }

        public void RemoveSimulationObserver(ISimulationObserver simulationObserver)
        {
            _simulationObservers.Remove(simulationObserver);
        }

        private void InvokeSimulationTickObservers(CommitResult commitResult, bool didRollBack)
        {
            foreach (var simulationObserver in _simulationObservers)
                simulationObserver.OnSimulationTick(commitResult, didRollBack);
            foreach (var entityViewObserver in _entityViewObservers)
                ((ISimulationObserver)entityViewObserver).OnSimulationTick(commitResult, didRollBack);
        }

        private void InvokeSimulationConfirmedTickObservers(int numTick, ArchetypeCollection tickData, CommitResult commitResult)
        {
            foreach (var simulationObserver in _simulationObservers)
                simulationObserver.OnSimulationConfirmedTick(numTick, tickData, commitResult);
            foreach (var entityViewObserver in _entityViewObservers)
                ((ISimulationObserver)entityViewObserver).OnSimulationConfirmedTick(numTick, tickData, commitResult);
        }

        private void InvokeSimulationRollbackObservers(int numFrames)
        {
            foreach (var simulationObserver in _simulationObservers)
                simulationObserver.OnSimulationRollback(numFrames);
            foreach (var entityViewObserver in _entityViewObservers)
                ((ISimulationObserver)entityViewObserver).OnSimulationRollback(numFrames);
        }
        #endregion

        #region EntityView
        public void AddEntityView<T1>(IEntityView<T1> entityView) 
            where T1 : struct
            => _entityViewObservers.Add(new EntityViewObserver<T1>(entityView, this, _logger));
        public void AddEntityView<T1, T2>(IEntityView<T1, T2> entityView)
            where T1 : struct where T2 : struct
            => _entityViewObservers.Add(new EntityViewObserver<T1, T2>(entityView, this, _logger));
        public void AddEntityView<T1, T2, T3>(IEntityView<T1, T2, T3> entityView)
            where T1 : struct where T2 : struct where T3 : struct
            => _entityViewObservers.Add(new EntityViewObserver<T1, T2, T3>(entityView, this, _logger));
        public void AddEntityView<T1, T2, T3, T4>(IEntityView<T1, T2, T3, T4> entityView)
            where T1 : struct where T2 : struct where T3 : struct where T4 : struct
            => _entityViewObservers.Add(new EntityViewObserver<T1, T2, T3, T4>(entityView, this, _logger));
        public void AddEntityView<T1, T2, T3, T4, T5>(IEntityView<T1, T2, T3, T4, T5> entityView)
            where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct
            => _entityViewObservers.Add(new EntityViewObserver<T1, T2, T3, T4, T5>(entityView, this, _logger));
        public void AddEntityView<T1, T2, T3, T4, T5, T6>(IEntityView<T1, T2, T3, T4, T5, T6> entityView)
            where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct
            => _entityViewObservers.Add(new EntityViewObserver<T1, T2, T3, T4, T5, T6>(entityView, this, _logger));
        public void AddEntityView<T1, T2, T3, T4, T5, T6, T7>(IEntityView<T1, T2, T3, T4, T5, T6, T7> entityView)
            where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct where T7 : struct
            => _entityViewObservers.Add(new EntityViewObserver<T1, T2, T3, T4, T5, T6, T7>(entityView, this, _logger));
        public void AddEntityView<T1, T2, T3, T4, T5, T6, T7, T8>(IEntityView<T1, T2, T3, T4, T5, T6, T7, T8> entityView)
            where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct where T7 : struct where T8 : struct
            => _entityViewObservers.Add(new EntityViewObserver<T1, T2, T3, T4, T5, T6, T7, T8>(entityView, this, _logger));


        public void UpdateEntityViewObservers()
        {
            foreach(var entityViewObserver in _entityViewObservers)
                entityViewObserver.Update();
        }

        #endregion

        #region Debug
        public DebugInfo GetDebugInfo()
        {
            return new DebugInfo()
            {
                MtuBytes = NetworkClient.Mtu,
                NetRttMs = NetworkClient.RoundTripTime,
                SimRtt = ClockSynchronizer.AvgRoundTripTime,
                SimJitter = ClockSynchronizer.RoundTripTimeStandardDeviation,
                SimLastSyncTime = Simulation.Clock.UtcNowRaw - ClockSynchronizer.LastSyncTime,
                SimNextSyncTime = ClockSynchronizer.NextSyncTime - Simulation.Clock.UtcNowRaw,
                SimClockOffset = Simulation.Clock.Offset,
                CurrentTick = Simulation.CurrentTick,
                ConfirmedTick = LastConfirmedTick,
            };
        }

        #endregion


        public void Dispose()
        {
            _networkClient.Disconnect();
            _networkClient.Dispose();
        }

    }
}
