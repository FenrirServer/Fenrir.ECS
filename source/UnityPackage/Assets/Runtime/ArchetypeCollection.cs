using Fenrir.Multiplayer;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Fenrir.ECS
{
    public class ArchetypeCollection : IByteStreamSerializable
    {
        /// <summary>
        /// Data version
        /// </summary>
        private const int _version = 1;

        /// <summary>
        /// List of registered archetypes
        /// </summary>
        private readonly List<ComponentCollection> _archetypes;

        /// <summary>
        /// Dictionary of component to an archetype in which it appears
        /// </summary>
        private readonly Dictionary<Type, List<ComponentCollection>> _componentToArchetypeList;

        public ComponentCollection this[int i] => _archetypes[i];

        public int NumArchetypes => _archetypes.Count;

        /// <summary>
        /// Last global entity id - used as a unique id
        /// </summary>
        private int LastEntityId;

        public ArchetypeCollection()
        {
            _archetypes = new List<ComponentCollection>();
            _componentToArchetypeList = new Dictionary<Type, List<ComponentCollection>>();
        }

        internal Entity CreateEntity(params Type[] componentTypes)
        {
            // Check if we have an archetype for this
            ComponentCollection archetype;

            if(!TryGetArchetype(out archetype, componentTypes))
            {
                archetype = CreateArchetype(componentTypes);
            }

            return archetype.AddEntity(LastEntityId++);
        }

        internal Entity CreateEntity<T1>()
            where T1 : struct
            => GetOrCreateArchetype(typeof(T1)).AddEntity(LastEntityId++);
        internal Entity CreateEntity<T1, T2>()
            where T1 : struct where T2 : struct
            => GetOrCreateArchetype(typeof(T1), typeof(T2)).AddEntity(LastEntityId++);
        internal Entity CreateEntity<T1, T2, T3>()
            where T1 : struct where T2 : struct where T3 : struct
            => GetOrCreateArchetype(typeof(T1), typeof(T2), typeof(T3)).AddEntity(LastEntityId++);
        internal Entity CreateEntity<T1, T2, T3, T4>()
            where T1 : struct where T2 : struct where T3 : struct where T4 : struct
            => GetOrCreateArchetype(typeof(T1), typeof(T2), typeof(T3), typeof(T4)).AddEntity(LastEntityId++);
        internal Entity CreateEntity<T1, T2, T3, T4, T5>()
            where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct
            => GetOrCreateArchetype(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5)).AddEntity(LastEntityId++);
        internal Entity CreateEntity<T1, T2, T3, T4, T5, T6>()
            where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct
            => GetOrCreateArchetype(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6)).AddEntity(LastEntityId++);
        internal Entity CreateEntity<T1, T2, T3, T4, T5, T6, T7>()
            where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct where T7 : struct
            => GetOrCreateArchetype(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7)).AddEntity(LastEntityId++);
        internal Entity CreateEntity<T1, T2, T3, T4, T5, T6, T7, T8>()
            where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct where T7 : struct where T8 : struct
            => GetOrCreateArchetype(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8)).AddEntity(LastEntityId++);


        internal void SetComponentData<T1>(Entity entity, T1 data) where T1 : struct
        {
            if (entity.ArchetypeId >= _archetypes.Count)
            {
                throw new ArgumentException($"Invalid entity archetype id: {entity.ArchetypeId}");
            }

            _archetypes[entity.ArchetypeId].SetComponentData(entity, data);
        }

        internal T1 GetComponentData<T1>(Entity entity) where T1 : struct
        {
            if (entity.ArchetypeId >= _archetypes.Count)
            {
                throw new ArgumentException($"Invalid entity archetype id: {entity.ArchetypeId}");
            }

            return _archetypes[entity.ArchetypeId].GetComponentData<T1>(entity);
        }

        internal void RemoveEntity(Entity entity)
        {
            if(entity.ArchetypeId >= _archetypes.Count)
            {
                throw new ArgumentException($"Invalid entity archetype id: {entity.ArchetypeId}");
            }

            if(_archetypes[entity.ArchetypeId].GetEntityId(entity.Index) != entity.Id)
            {
                throw new InvalidOperationException($"Attempting to remove entity {entity} but entity at index {entity.Index} has id {_archetypes[entity.ArchetypeId].GetEntityId(entity.Index)}");
            }

            _archetypes[entity.ArchetypeId].RemoveEntity(entity);
        }

        internal bool HasComponent<T1>(Entity entity)
        {
            return GetArchetype(entity.ArchetypeId).ComponentTypes.All(componentType =>
                componentType == typeof(T1)
            );
        }

        internal bool HasComponents(Entity entity, params Type[] componentTypes)
        {
            return GetArchetype(entity.ArchetypeId).HasComponents(componentTypes);
        }

        internal bool HasComponents<T1, T2>(Entity entity)
            where T1 : struct
            where T2 : struct
        {
            return GetArchetype(entity.ArchetypeId).ComponentTypes.All(componentType =>
                componentType == typeof(T1)
                || componentType == typeof(T2)
            );
        }

        internal bool HasComponents<T1, T2, T3>(Entity entity)
            where T1 : struct
            where T2 : struct
            where T3 : struct
        {
            return GetArchetype(entity.ArchetypeId).ComponentTypes.All(componentType =>
                componentType == typeof(T1)
                || componentType == typeof(T2)
                || componentType == typeof(T3)
            );
        }

        internal bool HasComponents<T1, T2, T3, T4>(Entity entity)
            where T1 : struct
            where T2 : struct
            where T3 : struct
            where T4 : struct
        {
            return GetArchetype(entity.ArchetypeId).ComponentTypes.All(componentType =>
                componentType == typeof(T1)
                || componentType == typeof(T2)
                || componentType == typeof(T3)
                || componentType == typeof(T4)
            );
        }
        internal bool HasComponents<T1, T2, T3, T4, T5>(Entity entity)
            where T1 : struct
            where T2 : struct
            where T3 : struct
            where T4 : struct
            where T5 : struct
        {
            return GetArchetype(entity.ArchetypeId).ComponentTypes.All(componentType =>
                componentType == typeof(T1)
                || componentType == typeof(T2)
                || componentType == typeof(T3)
                || componentType == typeof(T4)
                || componentType == typeof(T5)
            );
        }

        internal bool HasComponents<T1, T2, T3, T4, T5, T6>(Entity entity)
            where T1 : struct
            where T2 : struct
            where T3 : struct
            where T4 : struct
            where T5 : struct
            where T6 : struct
        {
            return GetArchetype(entity.ArchetypeId).ComponentTypes.All(componentType =>
                componentType == typeof(T1)
                || componentType == typeof(T2)
                || componentType == typeof(T3)
                || componentType == typeof(T4)
                || componentType == typeof(T5)
                || componentType == typeof(T6)
            );
        }

        internal bool HasComponents<T1, T2, T3, T4, T5, T6, T7>(Entity entity)
            where T1 : struct
            where T2 : struct
            where T3 : struct
            where T4 : struct
            where T5 : struct
            where T6 : struct
            where T7 : struct
        {
            return GetArchetype(entity.ArchetypeId).ComponentTypes.All(componentType =>
                componentType == typeof(T1)
                || componentType == typeof(T2)
                || componentType == typeof(T3)
                || componentType == typeof(T4)
                || componentType == typeof(T5)
                || componentType == typeof(T6)
                || componentType == typeof(T7)
            );
        }
        internal bool HasComponents<T1, T2, T3, T4, T5, T6, T7, T8>(Entity entity)
            where T1 : struct
            where T2 : struct
            where T3 : struct
            where T4 : struct
            where T5 : struct
            where T6 : struct
            where T7 : struct
            where T8 : struct
        {
            return GetArchetype(entity.ArchetypeId).ComponentTypes.All(componentType =>
                componentType == typeof(T1)
                || componentType == typeof(T2)
                || componentType == typeof(T3)
                || componentType == typeof(T4)
                || componentType == typeof(T5)
                || componentType == typeof(T6)
                || componentType == typeof(T7)
                || componentType == typeof(T8)
            );
        }


        internal bool TryGetComponentData<T1>(Entity entity, out T1 data) where T1 : struct
            => GetArchetype(entity.ArchetypeId).TryGetComponentData(entity, out data);

        internal bool TryGetComponentData<T1, T2>(Entity entity, out T1 data1, out T2 data2)
            where T1 : struct
            where T2 : struct
            => GetArchetype(entity.ArchetypeId).TryGetComponentData(entity, out data1, out data2);

        internal bool TryGetComponentData<T1, T2, T3>(Entity entity, out T1 data1, out T2 data2, out T3 data3)
            where T1 : struct
            where T2 : struct
            where T3 : struct
            => GetArchetype(entity.ArchetypeId).TryGetComponentData(entity, out data1, out data2, out data3);

        internal bool TryGetComponentData<T1, T2, T3, T4>(Entity entity, out T1 data1, out T2 data2, out T3 data3, out T4 data4)
            where T1 : struct
            where T2 : struct
            where T3 : struct
            where T4 : struct
            => GetArchetype(entity.ArchetypeId).TryGetComponentData(entity, out data1, out data2, out data3, out data4);

        internal bool TryGetComponentData<T1, T2, T3, T4, T5>(Entity entity, out T1 data1, out T2 data2, out T3 data3, out T4 data4, out T5 data5)
            where T1 : struct
            where T2 : struct
            where T3 : struct
            where T4 : struct
            where T5 : struct
            => GetArchetype(entity.ArchetypeId).TryGetComponentData(entity, out data1, out data2, out data3, out data4, out data5);

        internal bool TryGetComponentData<T1, T2, T3, T4, T5, T6>(Entity entity, out T1 data1, out T2 data2, out T3 data3, out T4 data4, out T5 data5, out T6 data6)
            where T1 : struct
            where T2 : struct
            where T3 : struct
            where T4 : struct
            where T5 : struct
            where T6 : struct
            => GetArchetype(entity.ArchetypeId).TryGetComponentData(entity, out data1, out data2, out data3, out data4, out data5, out data6);


        internal bool TryGetArchetype(out ComponentCollection archetype, params Type[] componentTypes)
        {
            archetype = null;

            foreach (var arch in _archetypes)
            {
                if (arch.HasComponents(componentTypes) && arch.NumComponents == componentTypes.Length)
                {
                    archetype = arch;
                    return true;
                }
            }

            return false;
        }

        internal ComponentCollection GetOrCreateArchetype(params Type[] componentTypes)
        {
            if(!TryGetArchetype(out ComponentCollection archetype, componentTypes))
            {
                archetype = CreateArchetype(componentTypes);
            }
            return archetype;
        }

        internal ComponentCollection CreateArchetype(Type[] componentTypes)
        {
            var archetype = new ComponentCollection(_archetypes.Count, componentTypes);

            _archetypes.Add(archetype);

            foreach(Type componentType in componentTypes)
            {
                if(!_componentToArchetypeList.ContainsKey(componentType))
                {
                    _componentToArchetypeList.Add(componentType, new List<ComponentCollection>());
                }

                _componentToArchetypeList[componentType].Add(archetype);
            }

            return archetype;
        }

        internal ComponentCollection GetArchetype(Type[] componentTypes)
        {
            for (int i = 0; i < _archetypes.Count; i++)
            {
                ComponentCollection archetype = _archetypes[i];
                if(archetype.HasComponents(componentTypes) && archetype.NumComponents == componentTypes.Length)
                {
                    return archetype;
                }
            }

            return null;
        }

        internal ComponentCollection GetArchetype(int archetypeId)
        {
            if(archetypeId >= _archetypes.Count)
            {
                return null;
            }
            return _archetypes[archetypeId];
        }


        internal void CopyTo(ArchetypeCollection nextSnapshot)
        {
            for (int i = 0; i < _archetypes.Count; i++)
            {
                ComponentCollection archetypeFrom = _archetypes[i];
                ComponentCollection archetypeTo = nextSnapshot.GetArchetype(archetypeFrom.Id);
                
                if(archetypeTo == null)
                {
                    archetypeTo = nextSnapshot.CreateArchetype(archetypeFrom.ComponentTypes);
                }

                archetypeTo.CopyFrom(archetypeFrom);
            }

            nextSnapshot.LastEntityId = LastEntityId; 
        }

        public ArchetypeEnumerable GetArchetypesContainingAll(params Type[] componentTypes)
        {
            return new ArchetypeEnumerable(this, ArchetypeQueryType.All, componentTypes);
        }

        public ArchetypeEnumerable GetArchetypesContainingAny(params Type[] componentTypes)
        {
            return new ArchetypeEnumerable(this, ArchetypeQueryType.Any, componentTypes);
        }

        public void Serialize(IByteStreamWriter writer)
        {
            writer.Write(_version);
            writer.Write(_archetypes);
        }

        public void Deserialize(IByteStreamReader reader)
        {
            int version = reader.ReadInt();

            var archetypes = reader.Read<List<ComponentCollection>>();

            // Create componentToArchetypeList
            foreach (var archetype in archetypes)
            {
                _archetypes.Add(archetype);

                foreach (Type componentType in archetype.ComponentTypes)
                {
                    if (!_componentToArchetypeList.ContainsKey(componentType))
                    {
                        _componentToArchetypeList.Add(componentType, new List<ComponentCollection>());
                    }
                    _componentToArchetypeList[componentType].Add(archetype);
                }
            }
        }
    }
}
