using Fenrir.Multiplayer;

namespace Fenrir.ECS
{
    public struct SimulationTickEvent<TInput> : IEvent, IByteStreamSerializable
        where TInput : struct, IByteStreamSerializable
    {
        public int NumTick;
        public TInput[] Inputs;

        public SimulationTickEvent(int numTick, TInput[] inputs)
        { 
            NumTick = numTick;
            Inputs = inputs;
        }

        public void Deserialize(IByteStreamReader reader)
        {
            NumTick = reader.ReadInt();
            int numPlayers = reader.ReadInt();
            Inputs = new TInput[numPlayers];
            for(int i=0; i<numPlayers;i++)
            {
                Inputs[i] = new TInput();
                Inputs[i].Deserialize(reader);
            }
        }

        public void Serialize(IByteStreamWriter writer)
        {
            writer.Write(NumTick);
            writer.Write(Inputs.Length);
            foreach (var playerInput in Inputs)
            {
                playerInput.Serialize(writer);
            }
        }
    }
}
