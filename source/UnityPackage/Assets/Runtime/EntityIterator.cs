namespace Fenrir.ECS
{
    public struct EntityIterator
    {
        public int NumEntities => _collection.NumEntities;

        private readonly ComponentCollection _collection;

        public Entity this[int index] => _collection.GetEntity(index);

        internal EntityIterator(ComponentCollection collection)
        {
            _collection = collection;
        }
    }
}
