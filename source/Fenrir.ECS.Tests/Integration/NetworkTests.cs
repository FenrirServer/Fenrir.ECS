using Fenrir.Multiplayer;
using FixedMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fenrir.ECS.Tests.Integration
{
    /*

    [TestClass]
    public class NetworkTests
    {
        [TestMethod, Timeout(30000)]
        public async Task TestSimulation()
        {
            var logger = Fixtures.CreateLogger();
            var networkServer = new NetworkServer(logger);

            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += (sender, e) =>
            {
                logger.Error(e.ExceptionObject.ToString());
            };

            // Create server
            Uri serverUri = new Uri($"http://localhost:{networkServer.BindPort}/");
            var mobaServer = new TestServer(logger, networkServer);
            mobaServer.Run().FireAndForget(logger);

            // Create clients
            var mobaClientA = new MobaClient(logger);
            var clientAObserver = new EventBasedObserver();
            mobaClientA.AddSimulationObserver(clientAObserver);
            await mobaClientA.Connect(serverUri, "test", "playerA");

            var mobaClientB = new MobaClient(logger);
            var clientBObserver = new EventBasedObserver();
            mobaClientB.AddSimulationObserver(clientBObserver);

            Entity clientB_thisEntity = default;
            Entity clientB_opponentEntity = default;

            TaskCompletionSource firstTickTcs = new TaskCompletionSource();
            TaskCompletionSource rollbackTcs = new TaskCompletionSource();
            TaskCompletionSource secondTickTcs = null;

            clientBObserver.RolledBack += (_, numFrames) =>
            {
                rollbackTcs.SetResult();
            };
            clientBObserver.Ticked += (_, commitResult) =>
            {
                if (mobaClientB.Simulation.CurrentTick == 1)
                {
                    // First tick spawned players, so record them
                    foreach (Entity entity in commitResult.AddedEntities)
                    {
                        if (mobaClientB.Simulation.ECSWorld.TryGetComponentData<PlayerComponent>(entity, out PlayerComponent playerComponent))
                        {
                            if (playerComponent.PlayerReference.PlayerId == mobaClientB.PlayerId)
                            {
                                clientB_thisEntity = entity;
                            }
                            else
                            {
                                clientB_opponentEntity = entity;
                            }
                        }
                    }

                    if (!firstTickTcs.Task.IsCompleted)
                    {
                        firstTickTcs.SetResult();
                    }
                }
                else if (mobaClientB.Simulation.CurrentTick == 2)
                {
                    if (!secondTickTcs?.Task?.IsCompleted ?? false)
                        secondTickTcs.SetResult();
                }
            };

            await mobaClientB.Connect(serverUri, "test", "playerB");

            // Select characters
            mobaClientA.SelectCharacter(0);
            mobaClientB.SelectCharacter(1);

            // Apply input as a client "A", moving right (90 degrees rotation full speed)
            mobaClientA.ApplyInput(new PlayerInput() { RotationRads = Fixed.PiTimes2, MovementVelocity = new FixedVector2(Fixed.One, Fixed.Zero) });

            await firstTickTcs.Task;
            await rollbackTcs.Task;

            secondTickTcs = new TaskCompletionSource();
            await secondTickTcs.Task;

            // Assert that opponent in client B moved right by 0.1M 
            var positionComponent = mobaClientB.Simulation.ECSWorld.GetComponentData<PositionComponent>(clientB_opponentEntity);
            var positionComponent2 = mobaClientB.Simulation.ECSWorld.GetComponentData<PositionComponent>(clientB_thisEntity);
            Assert.IsTrue(positionComponent.Position.X > Fixed.Zero);
        }
    }
    */
}
