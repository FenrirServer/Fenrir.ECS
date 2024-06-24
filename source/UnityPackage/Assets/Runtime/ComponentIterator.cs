using System.Runtime.CompilerServices;

namespace Fenrir.ECS
{
    public readonly ref struct ComponentIterator<T>
        where T : struct
    {
        public int NumEntities => _collection.NumEntities;

        private readonly ComponentCollection _collection;

        private readonly int _componentOffset;

        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return ref _collection.GetComponent<T>(index, _componentOffset);
            }
        }

        internal ComponentIterator(ComponentCollection collection)
        {
            _collection = collection;
            _componentOffset = collection.GetComponentOffset<T>();
        }
    }
}
