
namespace Fenrir.ECS
{
    public struct EntityCreateData
    {
        public Entity Entity;
        public int CurrentTick;

        public EntityCreateData(Entity entity, int currentTick)
        {
            Entity = entity;
            CurrentTick = currentTick;
        }
    }
}
