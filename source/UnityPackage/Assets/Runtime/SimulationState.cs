using Fenrir.Multiplayer;
using System;
using System.Collections.Generic;

namespace Fenrir.ECS
{
    /// <summary>
    /// Used to synchronize multiple simulations
    /// </summary>
    public struct SimulationState : IByteStreamSerializable
    {
        /// <summary>
        /// Version of the data
        /// </summary>
        private int _version;

        /// <summary>
        /// Number of the current tick
        /// </summary>
        public int CurrentTick;

        /// <summary>
        /// Time of the current tick
        /// </summary>
        public DateTime CurrentTickTime;

        /// <summary>
        /// Component data of the current tick
        /// </summary>
        public ArchetypeCollection CurrentTickData;

        /// <summary>
        /// Current players of the simulation
        /// </summary>
        public List<PlayerReference> Players;


        public void Deserialize(IByteStreamReader reader)
        {
            int version = reader.ReadInt();
            CurrentTick = reader.ReadInt();
            CurrentTickTime = new DateTime(reader.ReadLong());
            CurrentTickData = reader.Read<ArchetypeCollection>();
            Players = reader.Read<List<PlayerReference>>();
        }

        public void Serialize(IByteStreamWriter writer)
        {
            writer.Write(_version);
            writer.Write(CurrentTick);
            writer.Write(CurrentTickTime.Ticks);
            writer.Write(CurrentTickData);
            writer.Write(Players);
        }
    }
}
