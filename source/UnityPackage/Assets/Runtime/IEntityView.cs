namespace Fenrir.ECS
{
    public interface IEntityView
    {
        void OnEntityRollback(EntityRollbackData rollbackData) { }
        void OnEntityDestroyed(EntityDestroyData destroyData) { }
        void OnEntityConfirmedDestroyed(EntityConfirmedDestroyData confirmedDestroyData) { }
    }

    public interface IEntityView<T1> : IEntityView
        where T1 : struct
    {
        void OnEntityCreated(EntityCreateData createData, T1 componentData1) { }
        void OnEntityConfirmedCreated(EntityConfirmedCreateData createData, T1 componentData1) { }
        void OnEntityTick(EntityTickData tickData, T1 componentData1) { }
        void OnEntityConfirmedTick(EntityConfirmedTickData tickData, T1 componentData1) { }
    }

    public interface IEntityView<T1, T2> : IEntityView
        where T1 : struct
        where T2 : struct
    {
        void OnEntityCreated(EntityCreateData createData, T1 componentData1, T2 componentData2) { }
        void OnEntityConfirmedCreated(EntityConfirmedCreateData createData, T1 componentData1, T2 componentData2) { }
        void OnEntityTick(EntityTickData tickData, T1 componentData1, T2 componentData2) { }
        void OnEntityConfirmedTick(EntityConfirmedTickData tickData, T1 componentData1, T2 componentData2) { }
    }

    public interface IEntityView<T1, T2, T3> : IEntityView
        where T1 : struct
        where T2 : struct
        where T3 : struct
    {
        void OnEntityCreated(EntityCreateData createData, T1 componentData1, T2 componentData2, T3 componentData3) { }
        void OnEntityConfirmedCreated(EntityConfirmedCreateData createData, T1 componentData1, T2 componentData2, T3 componentData3) { }
        void OnEntityTick(EntityTickData tickData, T1 componentData1, T2 componentData2, T3 componentData3) { }
        void OnEntityConfirmedTick(EntityConfirmedTickData tickData, T1 componentData1, T2 componentData2, T3 componentData3) { }
    }

    public interface IEntityView<T1, T2, T3, T4> : IEntityView
        where T1 : struct
        where T2 : struct
        where T3 : struct
        where T4 : struct
    {
        void OnEntityCreated(EntityCreateData createData, T1 componentData1, T2 componentData2, T3 componentData3, T4 componentData4) { }
        void OnEntityConfirmedCreated(EntityConfirmedCreateData createData, T1 componentData1, T2 componentData2, T3 componentData3, T4 componentData4) { }
        void OnEntityTick(EntityTickData tickData, T1 componentData1, T2 componentData2, T3 componentData3, T4 componentData4) { }
        void OnEntityConfirmedTick(EntityConfirmedTickData tickData, T1 componentData1, T2 componentData2, T3 componentData3, T4 componentData4) { }
    }

    public interface IEntityView<T1, T2, T3, T4, T5> : IEntityView
        where T1 : struct
        where T2 : struct
        where T3 : struct
        where T4 : struct
        where T5 : struct
    {
        void OnEntityCreated(EntityCreateData createData, T1 componentData1, T2 componentData2, T3 componentData3, T4 componentData4, T5 componentData5) { }
        void OnEntityConfirmedCreated(EntityConfirmedCreateData createData, T1 componentData1, T2 componentData2, T3 componentData3, T4 componentData4, T5 componentData5) { }
        void OnEntityTick(EntityTickData tickData, T1 componentData1, T2 componentData2, T3 componentData3, T4 componentData4, T5 componentData5) { }
        void OnEntityConfirmedTick(EntityConfirmedTickData tickData, T1 componentData1, T2 componentData2, T3 componentData3, T4 componentData4, T5 componentData5) { }
    }

    public interface IEntityView<T1, T2, T3, T4, T5, T6> : IEntityView
        where T1 : struct
        where T2 : struct
        where T3 : struct
        where T4 : struct
        where T5 : struct
        where T6 : struct
    {
        void OnEntityCreated(EntityCreateData createData, T1 componentData1, T2 componentData2, T3 componentData3, T4 componentData4, T5 componentData5, T6 componentData6) { }
        void OnEntityConfirmedCreated(EntityConfirmedCreateData createData, T1 componentData1, T2 componentData2, T3 componentData3, T4 componentData4, T5 componentData5, T6 componentData6) { }
        void OnEntityTick(EntityTickData tickData, T1 componentData1, T2 componentData2, T3 componentData3, T4 componentData4, T5 componentData5, T6 componentData6) { }
        void OnEntityConfirmedTick(EntityConfirmedTickData tickData, T1 componentData1, T2 componentData2, T3 componentData3, T4 componentData4, T5 componentData5, T6 componentData6) { }
    }

    public interface IEntityView<T1, T2, T3, T4, T5, T6, T7> : IEntityView
        where T1 : struct
        where T2 : struct
        where T3 : struct
        where T4 : struct
        where T5 : struct
        where T6 : struct
        where T7 : struct
    {
        void OnEntityCreated(EntityCreateData createData, T1 componentData1, T2 componentData2, T3 componentData3, T4 componentData4, T5 componentData5, T6 componentData6, T7 componentData7) { }
        void OnEntityConfirmedCreated(EntityConfirmedCreateData createData, T1 componentData1, T2 componentData2, T3 componentData3, T4 componentData4, T5 componentData5, T6 componentData6, T7 componentData7) { }
        void OnEntityTick(EntityTickData tickData, T1 componentData1, T2 componentData2, T3 componentData3, T4 componentData4, T5 componentData5, T6 componentData6, T7 componentData7) { }
        void OnEntityConfirmedTick(EntityConfirmedTickData tickData, T1 componentData1, T2 componentData2, T3 componentData3, T4 componentData4, T5 componentData5, T6 componentData6, T7 componentData7) { }
    }

    public interface IEntityView<T1, T2, T3, T4, T5, T6, T7, T8> : IEntityView
        where T1 : struct
        where T2 : struct
        where T3 : struct
        where T4 : struct
        where T5 : struct
        where T6 : struct
        where T7 : struct
        where T8 : struct
    {
        void OnEntityCreated(EntityCreateData createData, T1 componentData1, T2 componentData2, T3 componentData3, T4 componentData4, T5 componentData5, T6 componentData6, T7 componentData7, T8 componentData8) { }
        void OnEntityConfirmedCreated(EntityConfirmedCreateData createData, T1 componentData1, T2 componentData2, T3 componentData3, T4 componentData4, T5 componentData5, T6 componentData6, T7 componentData7, T8 componentData8) { }
        void OnEntityTick(EntityTickData tickData, T1 componentData1, T2 componentData2, T3 componentData3, T4 componentData4, T5 componentData5, T6 componentData6, T7 componentData7, T8 componentData8) { }
        void OnEntityConfirmedTick(EntityConfirmedTickData tickData, T1 componentData1, T2 componentData2, T3 componentData3, T4 componentData4, T5 componentData5, T6 componentData6, T7 componentData7, T8 componentData8) { }
    }

}
