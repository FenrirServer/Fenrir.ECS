
namespace Fenrir.ECS
{
    struct EntityViewOperationData
    {
        public EntityViewOperation Operation;
        public Entity Entity;
        public int ConfirmedTick;
        public int CurrentTick;
        public bool DidRollback;
        public int NumRollbackTicks;

        public EntityViewOperationData(EntityCreateData createData) : this()
        {
            Operation = EntityViewOperation.Create;
            Entity = createData.Entity;
            CurrentTick = createData.CurrentTick;
        }

        public EntityViewOperationData(EntityConfirmedCreateData confirmedCreateData) : this()
        {
            Operation = EntityViewOperation.ConfirmedCreate;
            Entity = confirmedCreateData.Entity;
            ConfirmedTick = confirmedCreateData.ConfirmedTick;
        }

        public EntityViewOperationData(EntityTickData tickData) : this()
        {
            Operation = EntityViewOperation.Tick;
            Entity = tickData.Entity;
            CurrentTick = tickData.CurrentTick;
            DidRollback = tickData.DidRollback;
        }

        public EntityViewOperationData(EntityConfirmedTickData confirmedTickData) : this()
        {
            Operation = EntityViewOperation.ConfirmedTick;
            Entity = confirmedTickData.Entity;
            ConfirmedTick = confirmedTickData.ConfirmedTick;
        }

        public EntityViewOperationData(EntityRollbackData rollbackData) : this()
        {
            Operation = EntityViewOperation.Rollback;
            Entity = rollbackData.Entity;
            CurrentTick = rollbackData.CurrentTick;
            NumRollbackTicks = rollbackData.NumTicks;
        }

        public EntityViewOperationData(EntityDestroyData destroyData) : this()
        {
            Operation = EntityViewOperation.Destroy;
            Entity = destroyData.Entity;
            CurrentTick = destroyData.CurrentTick;
        }

        public EntityViewOperationData(EntityConfirmedDestroyData confirmedDestroyData) : this()
        {
            Operation = EntityViewOperation.ConfirmedDestroy;
            Entity = confirmedDestroyData.Entity;
            ConfirmedTick = confirmedDestroyData.ConfirmedTick;
        }

        public EntityCreateData CreateData => new EntityCreateData(Entity, CurrentTick);

        public EntityConfirmedCreateData ConfirmedCreateData => new EntityConfirmedCreateData(Entity, ConfirmedTick);

        public EntityTickData TickData => new EntityTickData(Entity, CurrentTick, DidRollback);

        public EntityConfirmedTickData ConfirmedTickData => new EntityConfirmedTickData(Entity, ConfirmedTick);

        public EntityRollbackData RollbackData => new EntityRollbackData(Entity, CurrentTick, NumRollbackTicks);

        public EntityDestroyData DestroyData => new EntityDestroyData(Entity, CurrentTick);

        public EntityConfirmedDestroyData ConfirmedDestroyData => new EntityConfirmedDestroyData(Entity, ConfirmedTick);
    }
}
