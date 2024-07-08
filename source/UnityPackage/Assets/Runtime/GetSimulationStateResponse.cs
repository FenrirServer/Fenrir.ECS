using Fenrir.Multiplayer;

namespace Fenrir.ECS
{
    public class GetSimulationStateResponse : IResponse, IByteStreamSerializable
    {
        private const int _version = 1;
        public bool Success;
        public SimulationState SimulationState;

        public GetSimulationStateResponse() { }

        public GetSimulationStateResponse(SimulationState simulationState)
        {
            Success = true;
            SimulationState = simulationState;
        }

        public void Deserialize(IByteStreamReader reader)
        {
            int version = reader.ReadInt();
            bool success = reader.ReadBool();
            SimulationState = reader.Read<SimulationState>();
        }

        public void Serialize(IByteStreamWriter writer)
        {
            writer.Write(_version);
            writer.Write(Success);
            writer.Write(SimulationState);
        }
    }
}
