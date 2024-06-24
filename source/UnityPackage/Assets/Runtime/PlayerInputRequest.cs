using Fenrir.Multiplayer;
using System;

namespace Fenrir.ECS
{
    /// <summary>
    /// Request that clients sends sever it's inputs
    /// </summary>
    public struct PlayerInputRequest<TInput> : IRequest, IByteStreamSerializable
        where TInput : struct, IByteStreamSerializable
    {
        public const byte Version = 1;

        public long NumTick;

        public TInput Input;

        public PlayerInputRequest(long numTick, TInput input)
        {
            NumTick = numTick;
            Input = input;
        }

        void IByteStreamSerializable.Deserialize(IByteStreamReader reader)
        {
            var version = reader.ReadByte();

            if (version != Version)
            {
                throw new InvalidOperationException("Invalid client input version");
            }

            NumTick = reader.ReadLong();
            Input = new TInput();
            Input.Deserialize(reader);
        }

        void IByteStreamSerializable.Serialize(IByteStreamWriter writer)
        {
            writer.Write(Version);
            writer.Write(NumTick);
            Input.Serialize(writer);
        }
    }
}
