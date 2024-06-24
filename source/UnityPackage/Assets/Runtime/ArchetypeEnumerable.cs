using System;
using System.Collections.Generic;

namespace Fenrir.ECS
{
    public ref struct ArchetypeEnumerable
    {
        private readonly ArchetypeCollection _archetypeCollection;

        private readonly ArchetypeQueryType _queryType;

        private readonly Type[] _componentTypes;

        internal ArchetypeEnumerable(ArchetypeCollection archetypeCollection, ArchetypeQueryType queryType, Type[] componentTypes)
        {
            _archetypeCollection = archetypeCollection;
            _queryType = queryType;
            _componentTypes = componentTypes;
        }

        public ArchetypeEnumerator GetEnumerator()
        {
            return new ArchetypeEnumerator(_archetypeCollection, _queryType, _componentTypes);
        }

        public int Count()
        {
            int n = 0;
            var enumerator = GetEnumerator();
            while(enumerator.MoveNext())
            {
                n++;
            }
            return n;
        }

        public List<ComponentCollection> ToList()
        {
            var list = new List<ComponentCollection>();
            var enumerator = GetEnumerator();
            while (enumerator.MoveNext())
            {
                list.Add(enumerator.Current);
            }
            return list;
        }
    }
}
