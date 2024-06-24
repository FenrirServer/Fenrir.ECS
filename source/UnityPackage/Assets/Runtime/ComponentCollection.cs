using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Fenrir.ECS
{
    public unsafe class ComponentCollection
    {
        private const int _initialSize = 16; // doubled every time we need more entities

        private readonly int _id;

        private readonly Type[] _componentTypes;

        private readonly HashSet<Type> _componentTypesHashSet;

        private readonly int[] _componentOffsets;

        private readonly int[] _componentSizes;

        private readonly Dictionary<Type, int> _componentIndexes;

        private readonly int _blockSizeBytes;

        private byte[] _componentData;

        private int _numEntities; // Current number of entities

        private int _sizeOfInt = Marshal.SizeOf<int>();

        internal Type[] ComponentTypes => _componentTypes;

        internal int DataLength => _componentData.Length;

        internal byte[] ComponentData => _componentData;

        public int NumEntities => _numEntities;

        public int NumComponents => _componentTypes.Length;


        public int Id => _id;

        public ComponentCollection(int id, params Type[] componentTypes)
        {
            // Check arguments
            if(componentTypes.Length == 0)
            {
                throw new ArgumentException($"{nameof(componentTypes)} can not be empty", nameof(componentTypes));
            }

            // Initialize
            _id = id;
            _componentTypes = componentTypes;
            _componentTypesHashSet = new HashSet<Type>(componentTypes);
            _componentSizes = new int[componentTypes.Length];
            _componentOffsets = new int[componentTypes.Length];
            _componentIndexes = new Dictionary<Type, int>();
            _blockSizeBytes = _sizeOfInt; // Entity id
            _numEntities = 0;

            // Calculate component sizes and offsets
            for (int numComponent = 0; numComponent < componentTypes.Length; numComponent++)
            {
                Type componentType = componentTypes[numComponent];
                _componentIndexes[componentType] = numComponent;
                int componentSize = Marshal.SizeOf(componentType);
                _componentSizes[numComponent] = componentSize;
                _componentOffsets[numComponent] = _blockSizeBytes;
                _blockSizeBytes += componentSize;
            }

            // Initialize component data array
            _componentData = new byte[_blockSizeBytes * _initialSize];
        }

        public bool HasComponentType(Type t) => _componentTypes.Contains(t);

        private void IncreaseSize()
        {
            Array.Resize(ref _componentData, _componentData.Length * 2);
        }

        private void IncreaseSize(int newSize)
        {
            Array.Resize(ref _componentData, newSize);
        }

        internal Entity AddEntity(int entityId)
        {
            // Add new entity to the end of the list
            int entityIndex = _numEntities;
            _numEntities++;

            // Check if we need to increase size
            if (_numEntities * _blockSizeBytes > _componentData.Length)
            {
                IncreaseSize();
            }

            // Write block at the next next free entity index
            int blockStartPos = entityIndex * _blockSizeBytes;

            // Clear the block bytes
            for (int i=0; i<_blockSizeBytes; i++)
            {
                _componentData[blockStartPos + i] = 0;
            }

            // Write unique entity id
            fixed (byte* dataBufferPtr = _componentData)
            {
                byte* entityPositionPtr = dataBufferPtr + entityIndex * _blockSizeBytes;
                Unsafe.Write(entityPositionPtr, entityId);
            }

            return new Entity(entityId, _id, entityIndex);
        }

        internal void RemoveEntity(Entity entity)
        {
            fixed (byte* dataBufferPtr = _componentData)
            {
                byte* entityPositionPtr = dataBufferPtr + entity.Index * _blockSizeBytes;

                // Iterate and shift all entities by 1 position to the left
                for(byte* ptr = entityPositionPtr; ptr < dataBufferPtr + (_numEntities-1) * _blockSizeBytes; ptr++)
                {
                    ptr[0] = ptr[_blockSizeBytes];
                }

                _numEntities--;
            }
        }

        public EntityIterator GetEntities()
        {
            return new EntityIterator(this);
        }

        public QueryResult<T1> GetComponents<T1>()
            where T1 : struct
        {
            return new QueryResult<T1>(
                new ComponentIterator<T1>(this),
                new EntityIterator(this)
                );
        }

        public QueryResult<T1, T2> GetComponents<T1, T2>()
            where T1 : struct
            where T2 : struct
        {
            return new QueryResult<T1, T2>(
                new ComponentIterator<T1>(this), 
                new ComponentIterator<T2>(this),
                new EntityIterator(this)
                );
        }
        public QueryResult<T1, T2, T3> GetComponents<T1, T2, T3>()
            where T1 : struct
            where T2 : struct
            where T3 : struct
        {
            return new QueryResult<T1, T2, T3>(
                new ComponentIterator<T1>(this), 
                new ComponentIterator<T2>(this), 
                new ComponentIterator<T3>(this),
                new EntityIterator(this)
                );
        }
        public QueryResult<T1, T2, T3, T4> GetComponents<T1, T2, T3, T4>()
            where T1 : struct
            where T2 : struct
            where T3 : struct
            where T4 : struct
        {
            return new QueryResult<T1, T2, T3, T4>(
                new ComponentIterator<T1>(this),
                new ComponentIterator<T2>(this),
                new ComponentIterator<T3>(this),
                new ComponentIterator<T4>(this),
                new EntityIterator(this)
                );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ref T GetComponent<T>(int index, int componentOffsetBytes) where T : struct
        {
            fixed (byte* dataBufferPtr = _componentData)
            {
                return ref Unsafe.AsRef<T>(dataBufferPtr + index * _blockSizeBytes + componentOffsetBytes);
            }
        }

        internal Entity GetEntity(int index)
        {
            int entityId = GetEntityId(index);
            return new Entity(entityId, Id, index);
        }

        internal int GetEntityId(int index)
        {
            fixed (byte* dataBufferPtr = _componentData)
            {
                return Unsafe.AsRef<int>(dataBufferPtr + index * _blockSizeBytes);
            }
        }

        internal int GetComponentOffset<T>() where T : struct
        {
            if (!_componentIndexes.ContainsKey(typeof(T)))
            {
                throw new ArgumentException("Component not found in archetype", typeof(T).Name);
            }

            int componentIndex = _componentIndexes[typeof(T)];
            return _componentOffsets[componentIndex];
        }

        internal bool HasComponent(Type componentType)
            => _componentTypesHashSet.Contains(componentType);

        internal bool HasComponents(Type[] componentTypes)
        {
            foreach(var componentType in componentTypes)
            {
                if(!HasComponent(componentType))
                {
                    return false;
                }
            }

            return true;
        }
        
        
        internal bool HasAnyComponents(Type[] componentTypes)
        {
            foreach(var componentType in componentTypes)
            {
                if(HasComponent(componentType))
                {
                    return true;
                }
            }

            return false;
        }

        internal void SetComponentData<T1>(Entity entity, T1 data) where T1 : struct
        {
            fixed (byte* dataBufferPtr = _componentData)
            {
                int componentOffset = GetComponentOffset<T1>();
                byte* componentPositionPtr = dataBufferPtr + entity.Index * _blockSizeBytes + componentOffset;
                Unsafe.Write<T1>(componentPositionPtr, data);
            }
        }

        public T1 GetComponentData<T1>(Entity entity) where T1 : struct
        {
            fixed (byte* dataBufferPtr = _componentData)
            {
                int componentOffset = GetComponentOffset<T1>();
                byte* componentPositionPtr = dataBufferPtr + entity.Index * _blockSizeBytes + componentOffset;
                return Unsafe.AsRef<T1>(componentPositionPtr);
            }
        }

        public bool TryGetComponentData<T1>(Entity entity, out T1 data1)
            where T1 : struct
        {
            data1 = default;
            if (!HasComponent(typeof(T1)))
                return false;
            data1 = GetComponentData<T1>(entity);
            return true;
        }

        public bool TryGetComponentData<T1, T2>(Entity entity, out T1 data1, out T2 data2)
            where T1 : struct
            where T2 : struct
        {
            data1 = default;
            data2 = default;
            if (!HasComponent(typeof(T1)) || !HasComponent(typeof(T2)))
                return false;
            data1 = GetComponentData<T1>(entity);
            data2 = GetComponentData<T2>(entity);
            return true;
        }

        public bool TryGetComponentData<T1, T2, T3>(Entity entity, out T1 data1, out T2 data2, out T3 data3)
            where T1 : struct
            where T2 : struct
            where T3 : struct
        {
            data1 = default;
            data2 = default;
            data3 = default;
            if (!HasComponent(typeof(T1)) || !HasComponent(typeof(T2)) || !HasComponent(typeof(T3)))
                return false;
            data1 = GetComponentData<T1>(entity);
            data2 = GetComponentData<T2>(entity);
            data3 = GetComponentData<T3>(entity);
            return true;
        }

        public bool TryGetComponentData<T1, T2, T3, T4>(Entity entity, out T1 data1, out T2 data2, out T3 data3, out T4 data4)
            where T1 : struct
            where T2 : struct
            where T3 : struct
            where T4 : struct
        {
            data1 = default;
            data2 = default;
            data3 = default;
            data4 = default;
            if (!HasComponent(typeof(T1)) || !HasComponent(typeof(T2)) || !HasComponent(typeof(T3)) || !HasComponent(typeof(T4)))
                return false;
            data1 = GetComponentData<T1>(entity);
            data2 = GetComponentData<T2>(entity);
            data3 = GetComponentData<T3>(entity);
            data4 = GetComponentData<T4>(entity);
            return true;
        }

        public bool TryGetComponentData<T1, T2, T3, T4, T5>(Entity entity, out T1 data1, out T2 data2, out T3 data3, out T4 data4, out T5 data5)
            where T1 : struct
            where T2 : struct
            where T3 : struct
            where T4 : struct
            where T5 : struct
        {
            data1 = default;
            data2 = default;
            data3 = default;
            data4 = default;
            data5 = default;
            if (!HasComponent(typeof(T1)) || !HasComponent(typeof(T2)) || !HasComponent(typeof(T3)) || !HasComponent(typeof(T4)) || !HasComponent(typeof(T5)))
                return false;
            data1 = GetComponentData<T1>(entity);
            data2 = GetComponentData<T2>(entity);
            data3 = GetComponentData<T3>(entity);
            data4 = GetComponentData<T4>(entity);
            data5 = GetComponentData<T5>(entity);
            return true;
        }


        public bool TryGetComponentData<T1, T2, T3, T4, T5, T6>(Entity entity, out T1 data1, out T2 data2, out T3 data3, out T4 data4, out T5 data5, out T6 data6)
            where T1 : struct
            where T2 : struct
            where T3 : struct
            where T4 : struct
            where T5 : struct
            where T6 : struct
        {
            data1 = default;
            data2 = default;
            data3 = default;
            data4 = default;
            data5 = default;
            data6 = default;
            if (!HasComponent(typeof(T1)) || !HasComponent(typeof(T2)) || !HasComponent(typeof(T3)) || !HasComponent(typeof(T4)) || !HasComponent(typeof(T5)) || !HasComponent(typeof(T6)))
                return false;
            data1 = GetComponentData<T1>(entity);
            data2 = GetComponentData<T2>(entity);
            data3 = GetComponentData<T3>(entity);
            data4 = GetComponentData<T4>(entity);
            data5 = GetComponentData<T5>(entity);
            data6 = GetComponentData<T6>(entity);
            return true;
        }

        internal void CopyFrom(ComponentCollection from)
        {
            // Copy all data from another component, resize if nessesary
            if(from.DataLength > _componentData.Length)
            {
                IncreaseSize(from.DataLength);
            }

            _numEntities = from.NumEntities;

            Array.Copy(from.ComponentData, _componentData, _numEntities * _blockSizeBytes);
        }

        public override string ToString()
        {
            return string.Join(",", _componentTypes.Select(t => t.Name));
        }
    }
}
