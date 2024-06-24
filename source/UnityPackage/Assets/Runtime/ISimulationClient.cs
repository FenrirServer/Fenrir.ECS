namespace Fenrir.ECS
{
    interface ISimulationClient
    {
        Simulation Simulation { get; }

        int LastConfirmedTick { get; }
    }
}
