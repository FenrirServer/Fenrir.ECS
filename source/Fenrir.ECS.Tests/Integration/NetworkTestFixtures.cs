using Fenrir.Multiplayer;
using Fenrir.Multiplayer.Rooms;
using WebSocketSharp;

namespace Fenrir.ECS.Tests.Integration
{
    /*
    internal class TestServer
        : IRequestHandler<PlayerInputRequest<PlayerInput>>
        , IRequestHandler<ClockSyncRequest>
    {
        private readonly TestLogger _logger;
        private readonly NetworkServer _networkServer;
        private readonly Clock _clock;

        TaskCompletionSource<int> _runTcs = new TaskCompletionSource<int>();


        public TestServer(TestLogger fenrirLogger, NetworkServer networkServer)
        {
            _logger = fenrirLogger;
            _networkServer = networkServer;
            _networkServer.UnsyncedEvents = true;
            _clock = new Clock();
        }

        public Task<int> Run()
        {
            // Add Moba room management
            _networkServer.AddRooms(CreateNewRoom);

            // Add request handlers
            _networkServer.AddRequestHandler<PlayerInputRequest<PlayerInput>>(this);
            _networkServer.AddRequestHandler<ClockSyncRequest>(this);

            // Start server
            _networkServer.Start();

            return _runTcs.Task;
        }

        private TestServerRoom CreateNewRoom(IServerPeer peer, string roomId, string joinToken)
        {
            return new TestServerRoom(_logger, roomId, _clock);
        }

        public Task Shutdown(int exitCode)
        {
            _runTcs.SetResult(exitCode);

            // Graceful shutdown: wait for all players to disconnect
            return Task.CompletedTask;
        }

        #region Request Handlers

        void IRequestHandler<PlayerInputRequest<PlayerInput>>.HandleRequest(PlayerInputRequest<PlayerInput> request, IServerPeer peer)
        {
            if (peer.PeerData == null)
            {
                // Not in the room
                return;
            }

            var player = (TestServerPlayer)peer.PeerData;

            // Get room
            var room = player.Room;

            // Dispatch message to the room
            room.Execute(() => room.HandlePlayerInputRequest(request, player));
        }

        void IRequestHandler<ClockSyncRequest>.HandleRequest(ClockSyncRequest request, IServerPeer peer)
        {
            var requestReceivedTime = _clock.UtcNow;
            var simulationClockSyncAckEvent = new ClockSyncAckEvent(request.RequestSentTime, requestReceivedTime);
            peer.SendEvent(simulationClockSyncAckEvent);
        }

        #endregion
    }


    class TestServerRoom : SimulationServerRoom<PlayerInput>
    {
        private TestServerPlayer[] _players;

        public TestServerRoom(ILogger logger, string roomId, Clock clock) : base(logger, roomId, clock)
        {
            StartGame();
        }

        protected override void OnPeerJoin(IServerPeer peer, string token)
        {
            var players = GetPlayers();
            var player = new TestServerPlayer(peer, this, (byte)(players.Length - 1));
            peer.PeerData = player;

            players = GetPlayers();
            if (players.Length > 1)
            {
                Schedule(() => // Send AFTER the connection handler is done
                {
                    peer.SendEvent(new PlayerJoinedEvent() { Player = players[0].GetPlayerReference() });
                    players[0].Peer.SendEvent(new PlayerJoinedEvent() { Player = player.GetPlayerReference() });
                }, 100);
            }
        }

        protected override void OnPeerLeave(IServerPeer peer)
        {
        }

        public void HandlePlayerInputRequest(PlayerInputRequest<PlayerInput> request, TestServerPlayer player)
        {
            IngestInput(player.GetPlayerReference(), request);
        }

        private TestServerPlayer[] GetPlayers()
        {
            TestServerPlayer[] players = Peers.Values.Select(peer => peer.PeerData)
                .Cast<TestServerPlayer>()
                .ToArray();

            return players;
        }

        private void StartGame()
        {
            _players = GetPlayers();
            PlayerReference[] playerReferences = _players.Select(player => player.GetPlayerReference()).ToArray();

            // Setup
            Simulation.AddSystem(new TestInputDispatchSystem(Simulation.ECSWorld, InputBuffer));
            Simulation.AddSystem(new TestMoveSystem(Simulation.ECSWorld));

            Schedule(() => RunSimulation(playerReferences), 200);
        }

        protected override PlayerInput PredictInput(PlayerInput previousInput)
        {
            return new PlayerInput()
            {
                MovementVelocity = previousInput.MovementVelocity,
            };
        }
    }

    class TestServerPlayer
    {
        public IServerPeer Peer { get; private set; }

        public byte PlayerId { get; private set; }

        public TestServerRoom Room { get; private set; }

        public TestServerPlayer(IServerPeer peer, TestServerRoom room, byte playerId)
        {
            Peer = peer;
            Room = room;
            PlayerId = playerId;
        }

        public PlayerReference GetPlayerReference()
        {
            return new PlayerReference() { PlayerId = PlayerId, PeerId = Guid.Parse(Peer.Id) };
        }
    }

    public class PlayerJoinedEvent : IEvent, IByteStreamSerializable
    {
        public PlayerReference Player;

        void IByteStreamSerializable.Deserialize(IByteStreamReader reader)
        {
            PlayerReference.Deserialize(reader, ref Player);
        }

        void IByteStreamSerializable.Serialize(IByteStreamWriter writer)
        {
            Player.Serialize(writer);
        }
    }

    public class TestClient
    : IEventHandler<PlayerJoinedEvent>
    , IDisposable
    {
        private readonly SimulationClient<PlayerInput> _simulationClient;
        private readonly ILogger _logger;

        internal Simulation Simulation => _simulationClient.Simulation;

        public event EventHandler<PlayerJoinedEvent> PlayerJoined;

        public int PlayerId => _simulationClient.ThisPlayer.PlayerId;


        public TestClient(ILogger logger)
        {
            _logger = logger;
            _simulationClient = new SimulationClient<PlayerInput>(logger);
            _simulationClient.PredictInputHandler = PredictInput;

            // Add event handlers
            _simulationClient.NetworkClient.AddEventHandler<PlayerJoinedEvent>(this);
        }

        public async Task Connect(Uri serverUri, string roomId, string playerName)
        {
            await _simulationClient.Connect(serverUri, roomId, playerName);

            _simulationClient.Simulation.AddSystem(new TestInputDispatchSystem(_simulationClient.Simulation.ECSWorld, _simulationClient.InputBuffer));
            _simulationClient.Simulation.AddSystem(new TestMoveSystem(_simulationClient.Simulation.ECSWorld));

            _simulationClient.Start(evt.Players, evt.SimulationStartTime);
        }

        public void Disconnect()
        {
            _simulationClient.Disconnect();
        }

        public void AddSimulationObserver(ISimulationObserver simulationObserver)
        {
            _simulationClient.AddSimulationObserver(simulationObserver);
        }

        public void RemoveSimulationObserver(ISimulationObserver simulationObserver)
        {
            _simulationClient.RemoveSimulationObserver(simulationObserver);
        }

        public void ApplyInput(PlayerInput playerInput)
        {
            _simulationClient.ApplyInput(playerInput);
        }

        private PlayerInput PredictInput(PlayerInput previousInput)
        {
            return new PlayerInput()
            {
                MovementVelocity = previousInput.MovementVelocity,
            };
        }

        #region Event Handlers
        void IEventHandler<PlayerJoinedEvent>.OnReceiveEvent(PlayerJoinedEvent evt)
        {
            PlayerJoined?.Invoke(this, evt);
        }

        #endregion

        public void Dispose()
        {
            _simulationClient?.Dispose();
        }
    }
    */
}
