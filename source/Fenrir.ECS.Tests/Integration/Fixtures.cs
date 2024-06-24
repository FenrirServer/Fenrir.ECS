using Fenrir.Multiplayer;
using FixedMath;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;
using System.Text;

namespace Fenrir.ECS.Tests.Integration
{

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct PlayerComponent
    {
        public PlayerReference PlayerReference;
        public PlayerInput CurrentInput;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct CounterComponent
    {
        public int N;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct PositionComponent
    {
        public Fixed X;
        public Fixed Y;
        public Fixed Z;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct VelocityComponent
    {
        public Fixed X;
        public Fixed Y;
        public Fixed Z;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct RotationComponent
    {
        public Fixed X;
        public Fixed Y;
        public Fixed Z;
        public Fixed W;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct SpinComponent
    {
        public Fixed X;
        public Fixed Y;
        public Fixed Z;
    }


    class PositionComponentCls
    {
        public Fixed X;
        public Fixed Y;
        public Fixed Z;
    }

    class VelocityComponentCls
    {
        public Fixed X;
        public Fixed Y;
        public Fixed Z;
    }

    class RotationComponentCls
    {
        public Fixed X;
        public Fixed Y;
        public Fixed Z;
        public Fixed W;
    }

    class SpinComponentCls
    {
        public Fixed X;
        public Fixed Y;
        public Fixed Z;
    }

    class TestCountSystem : ISystem
    {
        private World _ecsWorld;

        public TestCountSystem(World ecsWorld)
        {
            _ecsWorld = ecsWorld;
        }

        public void Tick()
        {
            var archetype = _ecsWorld.GetArchetype(typeof(CounterComponent));

            var (counters, numEntities) = archetype.GetComponents<CounterComponent>();

            for (int i = 0; i < numEntities; i++)
            {
                counters[i].N++;
            }
        }
    }

    class TestMoveSystem : ISystem
    {
        private World _ecsWorld;

        public TestMoveSystem(World ecsWorld)
        {
            _ecsWorld = ecsWorld;
        }

        public void Tick()
        {
            var archetypes = _ecsWorld.GetArchetypesContainingAll(typeof(PositionComponent), typeof(VelocityComponent));
            
            foreach(var archetype in archetypes)
            {
                var (positions, velocities, numEntities) = archetype.GetComponents<PositionComponent, VelocityComponent>();

                for(int i=0; i<numEntities; i++)
                {
                    positions[i].X += velocities[i].X;
                    positions[i].Y += velocities[i].Y;
                    positions[i].Z += velocities[i].Z;
                }
            }
        }
    }

    class TestSpinSystem : ISystem
    {
        private World _ecsWorld;

        public TestSpinSystem(World ecsWorld)
        {
            _ecsWorld = ecsWorld;
        }

        public void Tick()
        {
            var archetypes = _ecsWorld.GetArchetypesContainingAll(typeof(RotationComponent), typeof(SpinComponent));

            foreach (var archetype in archetypes)
            {
                var (rotations, spin, numEntities) = archetype.GetComponents<RotationComponent, SpinComponent>();

                for (int i = 0; i < numEntities; i++)
                {
                    rotations[i].X += spin[i].X;
                    rotations[i].Y += spin[i].Y;
                    rotations[i].Z += spin[i].Z;
                }
            }
        }
    }

    public class TestInputDispatchSystem : ISystem
    {
        private readonly World _ecsWorld;
        private readonly InputBuffer<PlayerInput> _inputBuffer;

        public TestInputDispatchSystem(World ecsWorld, InputBuffer<PlayerInput> inputBuffer)
        {
            _ecsWorld = ecsWorld;
            _inputBuffer = inputBuffer;
        }

        public void Tick()
        {
            var playerArchetypes = _ecsWorld.GetArchetypesContainingAll(typeof(PlayerComponent), typeof(VelocityComponent));

            foreach (var playerArchetype in playerArchetypes)
            {
                var (players, velocities, numPlayers) = playerArchetype.GetComponents<PlayerComponent, VelocityComponent>();

                for (int numPlayer = 0; numPlayer < numPlayers; numPlayer++)
                {
                    if (_inputBuffer.TryGetInput(players[numPlayer].PlayerReference.PlayerId, out PlayerInput playerInput))
                    {
                        players[numPlayer].CurrentInput = playerInput;
                    }

                    FixedVector2 movementVelocity = players[numPlayer].CurrentInput.MovementVelocity;

                    if (movementVelocity.Length() > Fixed.One)
                    {
                        movementVelocity = FixedVector2.Normalize(movementVelocity); // hacks?
                    }

                    // Movement velocity
                    velocities[numPlayer].X = movementVelocity.X;
                    velocities[numPlayer].Y = movementVelocity.Y;
                }
            }
        }
    }
    public struct PlayerInput : IByteStreamSerializable
    {
        public FixedVector2 MovementVelocity;

        public void Deserialize(IByteStreamReader reader)
        {
            MovementVelocity = new FixedVector2(
                Fixed.FromRaw(reader.ReadLong()),
                Fixed.FromRaw(reader.ReadLong())
            );
        }

        public void Serialize(IByteStreamWriter writer)
        {
            writer.Write(MovementVelocity.X.RawValue);
            writer.Write(MovementVelocity.Y.RawValue);
        }
    }

    internal static class Fixtures
    {
        public static TestLogger CreateLogger()
        {
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();

            var factory = serviceProvider.GetService<ILoggerFactory>();

            var logger = factory.CreateLogger<TestLogger>();

            return new TestLogger(logger);
        }
    }

    class EventBasedObserver : ISimulationObserver
    {
        public event EventHandler<CommitResult> Ticked;
        public event EventHandler<CommitResult> ConfirmedTick;
        public event EventHandler<int> RolledBack;

        public void OnSimulationTick(CommitResult commitResult, bool didRollback)
        {
            Ticked?.Invoke(this, commitResult);
        }

        public void OnSimulationConfirmedTick(int numTick, ArchetypeCollection tickData, CommitResult commitResult)
        {
            ConfirmedTick?.Invoke(this, commitResult);
        }

        public void OnSimulationRollback(int numTicks)
        {
            RolledBack?.Invoke(this, numTicks);
        }
    }

    class MockSimulationClient : ISimulationClient
    {
        public Simulation Simulation { get; set; }

        public int LastConfirmedTick { get; set; } = -1;

        public MockSimulationClient(Simulation simulation)
        {
            Simulation = simulation;
        }
    }

    class MockEntityView : IEntityView<PositionComponent>
    {
        public bool DidTick;
        public bool DidConfirmedTick;
        public bool DidRollback;
        public bool DidDestroy;
        public bool DidConfirmedDestroy;

        public EntityTickData LastTickData;
        public EntityConfirmedTickData LastConfirmedTickData;
        public PositionComponent LastTickPosition;
        public EntityRollbackData LastRollbackData;
        public EntityDestroyData LastConfirmedDestroyData;

        public void OnEntityTick(EntityTickData tickData, PositionComponent positionComponent)
        {
            LastTickData = tickData;
            LastTickPosition = positionComponent;
            DidTick = true;
        }
        public void OnEntityConfirmedTick(EntityConfirmedTickData tickData, PositionComponent componentData1)
        {
            LastConfirmedTickData = tickData;
        }
        public void OnEntityRollback(EntityRollbackData rollbackData)
        {
            LastRollbackData = rollbackData;
            DidRollback = true;
        }

        public void OnEntityDestroy(EntityDestroyData destroyData)
        {
            DidDestroy = true;
        }

        public void OnEntityConfirmedDestroy(EntityDestroyData confirmedDestroyData)
        {
            LastConfirmedDestroyData = confirmedDestroyData;
        }
    }
}
