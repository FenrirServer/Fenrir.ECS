using Fenrir.Multiplayer;
using FixedMath;

namespace Fenrir.ECS.Tests.Integration
{
    [TestClass]
    public class EntityViewTests
    {
        [TestMethod]
        public void Test_EntityViewObserver()
        {
            var logger = Fixtures.CreateLogger();
            var simulation = new Simulation(new Clock(), logger);
            var inputBuffer = new InputBuffer<PlayerInput>();
            var simulationClient = new MockSimulationClient(simulation);
            simulation.AddSystem(new TestInputDispatchSystem(simulation.ECSWorld, inputBuffer));
            simulation.AddSystem(new TestMoveSystem(simulation.ECSWorld));


            var entityView = new MockEntityView();

            var entityViewObserver = new EntityViewObserver<PositionComponent>(entityView, simulationClient, logger);

            var simulationObserver = entityViewObserver as ISimulationObserver;

            var player1 = new PlayerReference() { PeerId = Guid.NewGuid(), PlayerId = 0 };
            var player2 = new PlayerReference() { PeerId = Guid.NewGuid(), PlayerId = 1 };

            simulation.ECSWorld.CreateEntity(
                new PlayerComponent() { PlayerReference = player1 },
                new PositionComponent(),
                new VelocityComponent()
            );
            simulation.ECSWorld.CreateEntity(
                new PlayerComponent() { PlayerReference = player2 },
                new PositionComponent(),
                new VelocityComponent()
            );
            simulation.Commit();

            simulation.CaptureSnapshot();
            inputBuffer.SetInput(player1.PlayerId, new PlayerInput() { MovementVelocity = new FixedVector2(Fixed.One, Fixed.Zero) });
            inputBuffer.SetInput(player2.PlayerId, new PlayerInput() { MovementVelocity = new FixedVector2(Fixed.One, Fixed.Zero) });
            simulation.TickSystems();
            simulation.Commit();
            simulation.CurrentTick++;

            simulation.TickSystems();
            var tickResult = simulation.Commit();
            simulation.CurrentTick++;

            simulationObserver.OnSimulationTick(tickResult, false);
            entityViewObserver.Update();

            Assert.IsTrue(entityView.LastTickPosition.X > Fixed.Zero, "incorrect position " + entityView.LastTickPosition.X);
        }
    }
}
