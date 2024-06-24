using FixedMath;

namespace Fenrir.ECS.Tests.Integration
{
    [TestClass]
    public class IntegrationTests
    {
        [TestMethod]
        public void TestWorld()
        {
            var ecsWorld = new World();

            // Create entities and archetypes
            ecsWorld.CreateEntity(typeof(PositionComponent));
            ecsWorld.CreateEntity(typeof(PositionComponent), typeof(VelocityComponent));
            ecsWorld.CreateEntity(typeof(PositionComponent), typeof(VelocityComponent), typeof(RotationComponent),
                typeof(SpinComponent));

            var (addedEntities, _) = ecsWorld.Commit();

            Assert.AreEqual(3, addedEntities.Length);

            // Set component data
            ecsWorld.SetComponentData(addedEntities[0], new PositionComponent() { X = (Fixed)1, Y = (Fixed)2, Z = (Fixed)3 });

            ecsWorld.SetComponentData(addedEntities[1], new PositionComponent() { X = (Fixed)1, Y = (Fixed)2, Z = (Fixed)3 });
            ecsWorld.SetComponentData(addedEntities[1], new VelocityComponent() { X = (Fixed)5, Y = (Fixed)5, Z = (Fixed)5 });

            ecsWorld.SetComponentData(addedEntities[2], new PositionComponent() { X = (Fixed)1, Y = (Fixed)2, Z = (Fixed)3 });
            ecsWorld.SetComponentData(addedEntities[2], new VelocityComponent() { X = (Fixed)6, Y = (Fixed)6, Z = (Fixed)6 });
            ecsWorld.SetComponentData(addedEntities[2], new RotationComponent() { X = (Fixed)1, Y = (Fixed)2, Z = (Fixed)3 });
            ecsWorld.SetComponentData(addedEntities[2], new SpinComponent() { X = (Fixed)7, Y = (Fixed)7, Z = (Fixed)7 });

            // Run system
            var moveSystem = new TestMoveSystem(ecsWorld);
            moveSystem.Tick();

            Assert.AreEqual((Fixed)1, ecsWorld.GetComponentData<PositionComponent>(addedEntities[0]).X, "X changed when it should not have");
            Assert.AreEqual((Fixed)2, ecsWorld.GetComponentData<PositionComponent>(addedEntities[0]).Y, "Y changed when it should not have");
            Assert.AreEqual((Fixed)3, ecsWorld.GetComponentData<PositionComponent>(addedEntities[0]).Z, "Z changed when it should not have");

            Assert.AreEqual((Fixed)(1 + 5), ecsWorld.GetComponentData<PositionComponent>(addedEntities[1]).X, "X is not valid");
            Assert.AreEqual((Fixed)(2 + 5), ecsWorld.GetComponentData<PositionComponent>(addedEntities[1]).Y, "Y is not valid");
            Assert.AreEqual((Fixed)(3 + 5), ecsWorld.GetComponentData<PositionComponent>(addedEntities[1]).Z, "Z is not valid");

            var spinSystem = new TestSpinSystem(ecsWorld);
            spinSystem.Tick();

            Assert.AreEqual((Fixed)(1 + 6), ecsWorld.GetComponentData<PositionComponent>(addedEntities[2]).X, "X is not valid");
            Assert.AreEqual((Fixed)(2 + 6), ecsWorld.GetComponentData<PositionComponent>(addedEntities[2]).Y, "Y is not valid");
            Assert.AreEqual((Fixed)(3 + 6), ecsWorld.GetComponentData<PositionComponent>(addedEntities[2]).Z, "Z is not valid");

            Assert.AreEqual((Fixed)(1 + 7), ecsWorld.GetComponentData<RotationComponent>(addedEntities[2]).X, "X is not valid");
            Assert.AreEqual((Fixed)(2 + 7), ecsWorld.GetComponentData<RotationComponent>(addedEntities[2]).Y, "Y is not valid");
            Assert.AreEqual((Fixed)(3 + 7), ecsWorld.GetComponentData<RotationComponent>(addedEntities[2]).Z, "Z is not valid");
        }

        [TestMethod]
        public void TestQueries()
        {
            var ecsWorld = new World();

            ecsWorld.CreateEntity(typeof(PositionComponent));
            ecsWorld.CreateEntity(typeof(PositionComponent), typeof(VelocityComponent));
            ecsWorld.CreateEntity(typeof(PositionComponent), typeof(VelocityComponent));
            ecsWorld.CreateEntity(typeof(PositionComponent), typeof(VelocityComponent), typeof(RotationComponent), typeof(SpinComponent));
            ecsWorld.CreateEntity(typeof(PositionComponent), typeof(VelocityComponent), typeof(RotationComponent), typeof(SpinComponent));
            ecsWorld.CreateEntity(typeof(PositionComponent), typeof(VelocityComponent), typeof(RotationComponent), typeof(SpinComponent));
            ecsWorld.CreateEntity(typeof(RotationComponent), typeof(SpinComponent));
            ecsWorld.CreateEntity(typeof(RotationComponent), typeof(SpinComponent));
            ecsWorld.CreateEntity(typeof(RotationComponent), typeof(SpinComponent));


            var (addedEntities, _) = ecsWorld.Commit();

            // Query all entities with a VelocityComponent
            var archetypes = ecsWorld.GetArchetypesContainingAll(typeof(VelocityComponent));
            // should return 2 archetypes with 2 and 3 entities respectively
            Assert.AreEqual(2, archetypes.Count());
            Assert.AreEqual(2, archetypes.ToList()[0].NumEntities);
            Assert.AreEqual(3, archetypes.ToList()[1].NumEntities);

            // Query all entities with a VelocityComponent and a RotationComponent
            archetypes = ecsWorld.GetArchetypesContainingAll(typeof(VelocityComponent), typeof(RotationComponent));
            // should return 1 archetype with 3 entities
            Assert.AreEqual(1, archetypes.Count());
            Assert.AreEqual(3, archetypes.ToList()[0].NumEntities);

            // Query all entities with a VelocityComponent and a RotationComponent and a SpinComponent
            var archetypes_exact = ecsWorld.GetArchetype(typeof(PositionComponent), typeof(VelocityComponent));

            // should return exactly 1 archetype with 2 entities and 2 components
            Assert.AreEqual(2, archetypes_exact.NumEntities);
            Assert.AreEqual(2, archetypes_exact.NumComponents);

            // Query all entities with a VelocityComponent or a RotationComponent
            archetypes = ecsWorld.GetArchetypesContainingAny(typeof(VelocityComponent), typeof(SpinComponent));
            // should return 3 archetypes with 2, 3 and 3 entities respectively
            Assert.AreEqual(3, archetypes.Count());
            Assert.AreEqual(2, archetypes.ToList()[0].NumEntities);
            Assert.AreEqual(3, archetypes.ToList()[1].NumEntities);
            Assert.AreEqual(3, archetypes.ToList()[2].NumEntities);
        }


        [TestMethod]
        public void TestRollback()
        {
            var ecsWorld = new World();
            var countSystem = new TestCountSystem(ecsWorld);

            // Create entity
            ecsWorld.CreateEntity(typeof(CounterComponent));
            var (addedEntities, _) = ecsWorld.Commit();
            Assert.AreEqual(1, addedEntities.Length);

            for (int i = 0; i < 20; i++)
            {
                ecsWorld.CaptureSnapshot();
                countSystem.Tick();
                Assert.AreEqual(i+1, ecsWorld.GetComponentData<CounterComponent>(addedEntities[0]).N);
            }

            // Roll 5 frames back to n=15
            ecsWorld.Rollback(5);
            Assert.AreEqual(15, ecsWorld.GetComponentData<CounterComponent>(addedEntities[0]).N);

            ecsWorld.CaptureSnapshot();
            countSystem.Tick();

            Assert.AreEqual(16, ecsWorld.GetComponentData<CounterComponent>(addedEntities[0]).N);
        }


        [TestMethod]
        public void TestEntityId()
        {
            var ecsWorld = new World();

            // Create entities and archetypes
            ecsWorld.CreateEntity(typeof(PositionComponent));
            ecsWorld.CreateEntity(typeof(PositionComponent), typeof(RotationComponent));
            ecsWorld.CreateEntity(typeof(PositionComponent));

            var (addedEntities, _) = ecsWorld.Commit();

            Assert.AreEqual(3, addedEntities.Length);

            Assert.AreEqual(0, addedEntities[0].Id, "Wrong entity id");
            Assert.AreEqual(1, addedEntities[1].Id, "Wrong entity id");
            Assert.AreEqual(2, addedEntities[2].Id, "Wrong entity id");

            // Set component data
            ecsWorld.SetComponentData(addedEntities[0], new PositionComponent() { X = (Fixed)1, Y = (Fixed)2, Z = (Fixed)3 });
            ecsWorld.SetComponentData(addedEntities[1], new PositionComponent() { X = (Fixed)4, Y = (Fixed)5, Z = (Fixed)6 });
            ecsWorld.SetComponentData(addedEntities[2], new PositionComponent() { X = (Fixed)7, Y = (Fixed)8, Z = (Fixed)9 });

            // Get only components for the first archetype, only entity 0 and 2
            var archetype = ecsWorld.GetArchetype(typeof(PositionComponent));

            var (positions, numEntities, entities) = archetype.GetComponents<PositionComponent>();

            Assert.AreEqual((Fixed)1, positions[0].X, "Wrong position X");
            Assert.AreEqual((Fixed)9, positions[1].Z, "Wrong position X");

            Assert.AreEqual(2, numEntities, "Wrong number of entities");

            Assert.AreEqual(0, entities[0].Id, "wrong entity id");
            Assert.AreEqual(2, entities[1].Id, "wrong entity id");
        }
    }
}
