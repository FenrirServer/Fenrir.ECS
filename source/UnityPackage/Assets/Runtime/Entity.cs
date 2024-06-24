using System.Collections.Generic;

namespace Fenrir.ECS
{
    public struct Entity : IEqualityComparer<Entity>
    {
        public int ArchetypeId;

        public int Index;

        public int Id;

        public Entity(int id, int archetypeId, int index)
        {
            Id = id;
            ArchetypeId = archetypeId;
            Index = index;
        }
        public bool Equals(Entity a, Entity b)
        {
            return a.ArchetypeId == b.ArchetypeId
                && a.Index == b.Index
                && a.Id == b.Id;
        }

        public int GetHashCode(Entity e) => (ArchetypeId, Index, Id).GetHashCode();

        public override string ToString()
        {
            return $"(AID={ArchetypeId},IDX={Index},ID={Id})";
        }
    }
}
