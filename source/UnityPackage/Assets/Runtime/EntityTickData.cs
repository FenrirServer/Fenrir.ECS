
namespace Fenrir.ECS
{
    public struct EntityTickData
    {
        public Entity Entity;
        public int CurrentTick;
        public bool DidRollback;

        public EntityTickData(Entity entity, int currentTick, bool didRollback)
        {
            Entity = entity;
            CurrentTick = currentTick;
            DidRollback = didRollback;
        }
    }
}
