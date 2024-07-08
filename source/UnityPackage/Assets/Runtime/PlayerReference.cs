using Fenrir.Multiplayer;
using System;

namespace Fenrir.ECS
{
    /// <summary>
    /// Represents Player Reference 
    /// sent over the wire
    /// </summary>
    public struct PlayerReference : IByteStreamSerializable
    {
        /// <summary>
        /// Data version
        /// </summary>
        private const int _version  = 1;

        /// <summary>
        /// Number of this player
        /// </summary>
        public byte PlayerId { get; set; }

        /// <summary>
        /// Id of the peer
        /// </summary>
        public Guid PeerId { get; set; }

        public void Deserialize(IByteStreamReader reader)
        {
            int version = reader.ReadByte();
            PlayerId = reader.ReadByte();
            PeerId = Guid.Parse(reader.ReadString());
        }

        public void Serialize(IByteStreamWriter writer)
        {
            writer.Write(_version);
            writer.Write(PlayerId);
            writer.Write(PeerId.ToString());
        }
    }
}
