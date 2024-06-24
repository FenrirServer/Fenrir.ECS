using Fenrir.Multiplayer;
using System;

namespace Fenrir.ECS
{
    /// <summary>
    /// Represents Player Reference 
    /// sent over the wire
    /// </summary>
    public struct PlayerReference
    {
        /// <summary>
        /// Number of this player
        /// </summary>
        public byte PlayerId { get; set; }

        /// <summary>
        /// Id of the peer
        /// </summary>
        public Guid PeerId { get; set; }

        public static void Deserialize(IByteStreamReader reader, ref PlayerReference player)
        {
            player.PlayerId = reader.ReadByte();
            player.PeerId = Guid.Parse(reader.ReadString());
        }

        public void Serialize(IByteStreamWriter writer)
        {
            writer.Write(PlayerId);
            writer.Write(PeerId.ToString());
        }
    }
}
