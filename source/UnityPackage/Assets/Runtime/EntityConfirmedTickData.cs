namespace Fenrir.ECS
{
    public struct EntityConfirmedTickData
    {
        public Entity Entity;
        public int ConfirmedTick;

        public EntityConfirmedTickData(Entity entity, int confirmedTick)
        {
            Entity = entity;
            ConfirmedTick = confirmedTick;
        }
    }
}
