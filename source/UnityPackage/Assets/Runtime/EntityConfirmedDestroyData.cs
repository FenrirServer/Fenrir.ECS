
namespace Fenrir.ECS
{
    public struct EntityConfirmedDestroyData
    {
        public Entity Entity;
        public int ConfirmedTick;

        public EntityConfirmedDestroyData(Entity entity, int currentTick)
        {
            Entity = entity;
            ConfirmedTick = currentTick;
        }
    }
}
