using Fenrir.Multiplayer;
using System;

namespace Fenrir.ECS
{
    public class ClockSyncRequest : IRequest, IByteStreamSerializable
    {
        public DateTime RequestSentTime;

        public ClockSyncRequest()
        {
        }

        public ClockSyncRequest(DateTime clientTime)
        {
            RequestSentTime = clientTime;
        }

        public void Deserialize(IByteStreamReader reader)
        {
            RequestSentTime = new DateTime(reader.ReadLong());
        }

        public void Serialize(IByteStreamWriter writer)
        {
            writer.Write(RequestSentTime.Ticks);
        }
    }
}
