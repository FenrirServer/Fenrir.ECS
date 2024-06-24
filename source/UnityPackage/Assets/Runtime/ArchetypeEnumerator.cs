using System;

namespace Fenrir.ECS
{
    public ref struct ArchetypeEnumerator
    {
        private readonly ArchetypeCollection _archetypeCollection;

        private readonly ArchetypeQueryType _queryType;

        private readonly Type[] _componentTypes;

        private int _index;

        internal ArchetypeEnumerator(ArchetypeCollection archetypeCollection, ArchetypeQueryType queryType, params Type[] componentTypes)
        {
            _archetypeCollection = archetypeCollection;
            _queryType = queryType;
            _componentTypes = componentTypes;
            _index = -1;
        }

        public ComponentCollection Current => _archetypeCollection[_index];

        public bool MoveNext()
        {
            while (true)
            {
                _index++;

                if (_index >= _archetypeCollection.NumArchetypes)
                {
                    return false;
                }

                ComponentCollection archetype = _archetypeCollection[_index];

                bool matches = false;

                switch(_queryType)
                {
                    case ArchetypeQueryType.All:
                        matches = archetype.HasComponents(_componentTypes);
                        break;
                    case ArchetypeQueryType.Any:
                        matches = archetype.HasAnyComponents(_componentTypes);
                        break;
                    default:
                        break;
                }

                if (matches)
                {
                    return true;
                }
                else
                {
                    // Continue moving the index, until we either find a matching archetype, or run out of archetypes
                    continue;
                }
            }
        }

        public void Reset()
        {
            _index = 0;
        }
    }
}
