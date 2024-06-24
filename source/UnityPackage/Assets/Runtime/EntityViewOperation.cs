namespace Fenrir.ECS
{
    enum EntityViewOperation
    {
        Create,
        ConfirmedCreate,
        Tick,
        ConfirmedTick,
        Rollback,
        Destroy,
        ConfirmedDestroy
    }
}
