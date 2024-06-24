namespace Fenrir.ECS
{
    public interface ISimulationClient
    {
        Simulation Simulation { get; }

        int LastConfirmedTick { get; }
    }
}
