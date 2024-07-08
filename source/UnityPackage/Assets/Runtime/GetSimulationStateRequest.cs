using Fenrir.Multiplayer;

namespace Fenrir.ECS
{
    public class GetSimulationStateRequest : IRequest<GetSimulationStateResponse>, IByteStreamSerializable
    {
        public void Deserialize(IByteStreamReader reader)
        {
        }

        public void Serialize(IByteStreamWriter writer)
        {
        }
    }
}
