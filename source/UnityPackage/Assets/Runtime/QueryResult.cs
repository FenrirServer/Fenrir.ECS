namespace Fenrir.ECS
{
    public readonly ref struct QueryResult<T1> 
        where T1 : struct
    {
        public readonly int NumEntities;
        private readonly ComponentIterator<T1> _iterator1;
        private readonly EntityIterator _entityIterator;

        public QueryResult(ComponentIterator<T1> iterator1, EntityIterator entityIterator)
        {
            NumEntities = iterator1.NumEntities;
            _iterator1 = iterator1;
            _entityIterator = entityIterator;
        }

        public void Deconstruct(out ComponentIterator<T1> iterator1, out int numEntities)
        {
            iterator1 = _iterator1;
            numEntities = _iterator1.NumEntities;
        }

        public void Deconstruct(out ComponentIterator<T1> iterator1, out int numEntities, out EntityIterator entityIterator)
        {
            iterator1 = _iterator1;
            numEntities = _iterator1.NumEntities;
            entityIterator = _entityIterator;
        }
    }

    public readonly ref struct QueryResult<T1, T2>
        where T1 : struct
        where T2 : struct
    {
        public readonly int NumEntities;
        private readonly ComponentIterator<T1> _iterator1;
        private readonly ComponentIterator<T2> _iterator2;
        private readonly EntityIterator _entityIterator;

        public QueryResult(ComponentIterator<T1> iterator1, ComponentIterator<T2> iterator2, EntityIterator entityIterator)
        {
            NumEntities = iterator1.NumEntities;
            _iterator1 = iterator1;
            _iterator2 = iterator2;
            _entityIterator = entityIterator;
        }

        public void Deconstruct(out ComponentIterator<T1> iterator1, out ComponentIterator<T2> iterator2, out int numEntities)
        {
            numEntities = _iterator1.NumEntities;
            iterator1 = _iterator1;
            iterator2 = _iterator2;
        }

        public void Deconstruct(out ComponentIterator<T1> iterator1, out ComponentIterator<T2> iterator2, out int numEntities, out EntityIterator entityIterator)
        {
            numEntities = _iterator1.NumEntities;
            iterator1 = _iterator1;
            iterator2 = _iterator2;
            entityIterator = _entityIterator;
        }
    }


    public readonly ref struct QueryResult<T1, T2, T3>
        where T1 : struct
        where T2 : struct
        where T3 : struct
    {
        public readonly int NumEntities;
        private readonly ComponentIterator<T1> _iterator1;
        private readonly ComponentIterator<T2> _iterator2;
        private readonly ComponentIterator<T3> _iterator3;
        private readonly EntityIterator _entityIterator;

        public QueryResult(ComponentIterator<T1> iterator1, ComponentIterator<T2> iterator2, ComponentIterator<T3> iterator3, EntityIterator entityIterator)
        {
            NumEntities = iterator1.NumEntities;
            _iterator1 = iterator1;
            _iterator2 = iterator2;
            _iterator3 = iterator3;
            _entityIterator = entityIterator;
        }

        public void Deconstruct(out ComponentIterator<T1> iterator1, out ComponentIterator<T2> iterator2, out ComponentIterator<T3> iterator3, out int numEntities)
        {
            iterator1 = _iterator1;
            iterator2 = _iterator2;
            iterator3 = _iterator3;
            numEntities = _iterator1.NumEntities;
        }

        public void Deconstruct(out ComponentIterator<T1> iterator1, out ComponentIterator<T2> iterator2, out ComponentIterator<T3> iterator3, out int numEntities, out EntityIterator entityIterator)
        {
            iterator1 = _iterator1;
            iterator2 = _iterator2;
            iterator3 = _iterator3;
            numEntities = _iterator1.NumEntities;
            entityIterator = _entityIterator;
        }
    }


    public readonly ref struct QueryResult<T1, T2, T3, T4>
        where T1 : struct
        where T2 : struct
        where T3 : struct
        where T4 : struct
    {
        public readonly int NumEntities;
        private readonly ComponentIterator<T1> _iterator1;
        private readonly ComponentIterator<T2> _iterator2;
        private readonly ComponentIterator<T3> _iterator3;
        private readonly ComponentIterator<T4> _iterator4;
        private readonly EntityIterator _entityIterator;

        public QueryResult(ComponentIterator<T1> iterator1, ComponentIterator<T2> iterator2, ComponentIterator<T3> iterator3, ComponentIterator<T4> iterator4, EntityIterator entityIterator)
        {
            NumEntities = iterator1.NumEntities;
            _iterator1 = iterator1;
            _iterator2 = iterator2;
            _iterator3 = iterator3;
            _iterator4 = iterator4;
            _entityIterator = entityIterator;
        }

        public void Deconstruct(out ComponentIterator<T1> iterator1, out ComponentIterator<T2> iterator2, out ComponentIterator<T3> iterator3, out ComponentIterator<T4> iterator4, out int numEntities)
        {
            iterator1 = _iterator1;
            iterator2 = _iterator2;
            iterator3 = _iterator3;
            iterator4 = _iterator4;
            numEntities = _iterator1.NumEntities;
        }

        public void Deconstruct(out ComponentIterator<T1> iterator1, out ComponentIterator<T2> iterator2, out ComponentIterator<T3> iterator3, out ComponentIterator<T4> iterator4, out int numEntities, out EntityIterator entityIterator)
        {
            iterator1 = _iterator1;
            iterator2 = _iterator2;
            iterator3 = _iterator3;
            iterator4 = _iterator4;
            numEntities = _iterator1.NumEntities;
            entityIterator = _entityIterator;
        }
    }
}
