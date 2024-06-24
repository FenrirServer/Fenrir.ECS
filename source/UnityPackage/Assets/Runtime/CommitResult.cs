using System.Collections.Generic;

namespace Fenrir.ECS
{
    public struct CommitResult
    {
        public Entity[] AddedEntities;
        public Entity[] RemovedEntities;

        public CommitResult(Entity[] addedEntities, Entity[] removedEntities)
        {
            AddedEntities = addedEntities;
            RemovedEntities = removedEntities;
        }

        public void Deconstruct(out Entity[] AddedEntities, out Entity[] removedEntities)
        {
            AddedEntities = this.AddedEntities;
            removedEntities = RemovedEntities;
        }
    }
}
