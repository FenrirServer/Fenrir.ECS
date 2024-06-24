
namespace Fenrir.ECS
{
    public struct EntityRollbackData
    {
        public Entity Entity;
        public int CurrentTick;
        public int NumTicks;

        public EntityRollbackData(Entity entity, int currentTick, int numTicks)
        {
            Entity = entity;
            CurrentTick = currentTick;
            NumTicks = numTicks;
        }
    }
}
