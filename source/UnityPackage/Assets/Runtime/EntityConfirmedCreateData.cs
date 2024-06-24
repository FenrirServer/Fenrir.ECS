
namespace Fenrir.ECS
{
    public struct EntityConfirmedCreateData
    {
        public Entity Entity;
        public int ConfirmedTick;

        public EntityConfirmedCreateData(Entity entity, int confirmedTick)
        {
            Entity = entity;
            ConfirmedTick = confirmedTick;
        }
    }
}
