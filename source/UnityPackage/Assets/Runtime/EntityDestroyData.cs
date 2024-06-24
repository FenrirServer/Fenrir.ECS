
namespace Fenrir.ECS
{
    public struct EntityDestroyData
    {
        public Entity Entity;
        public int CurrentTick;

        public EntityDestroyData(Entity entity, int currentTick)
        {
            Entity = entity;
            CurrentTick = currentTick;
        }
    }
}
