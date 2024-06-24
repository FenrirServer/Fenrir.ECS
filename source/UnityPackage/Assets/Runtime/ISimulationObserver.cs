namespace Fenrir.ECS
{
    public interface ISimulationObserver
    {
        void OnSimulationTick(CommitResult commitResult, bool didRollBack) { }

        void OnSimulationConfirmedTick(int numTick, ArchetypeCollection tickData, CommitResult commitResult) { }

        void OnSimulationRollback(int numTicks) { }
    }
}
