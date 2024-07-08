using Fenrir.ECS.Operations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Fenrir.ECS
{
    public class World
    {
        private List<Func<Entity>> _addEntityOperations;
        private List<AddArchetypeOperation> _addArchetypeOperations;

        // Archetype id => entity index => entity
        private Dictionary<int, SortedDictionary<int, Entity>> _removeEntityOperations;

        private readonly LinkedList<TickSnapshot> _snapshots;
        private readonly LinkedList<TickSnapshot> _snapshootPool;

        // TODO Change to internal. It should be hidden
        public ArchetypeCollection CurrentTickData => _snapshots.Last.Value.TickData;

        public CommitResult CurrentCommitResult => _snapshots.Last.Value.CommitResult;

        public int NumSnapshots => _snapshots.Count;

        public object Tag { get; set; }

        public World(int initialSnapshotPoolSize = 50)
        {
            _snapshots = new LinkedList<TickSnapshot>();
            _snapshootPool = new LinkedList<TickSnapshot>();

            _snapshots.AddLast(new TickSnapshot() { TickData = new ArchetypeCollection() });

            for (int i= 0; i < initialSnapshotPoolSize; i++)
            {
                _snapshootPool.AddLast(new TickSnapshot() { TickData = new ArchetypeCollection() });
            }

            _addEntityOperations = new List<Func<Entity>>();
            _addArchetypeOperations = new List<AddArchetypeOperation>();
            _removeEntityOperations = new Dictionary<int, SortedDictionary<int, Entity>>();
        }

        public void CreateArchetype(params Type[] componentTypes)
        {
            foreach (var componentType in componentTypes)
            {
                if (!componentType.IsUnmanagedStruct())
                {
                    throw new ArgumentException($"Can not use type {componentType.Name} as component type");
                }
            }

            // Add to the list of operations
            _addArchetypeOperations.Add(new AddArchetypeOperation(componentTypes));
        }

        private void ThrowIfNotUnmanaged(Type componentType)
        {
            if (!componentType.IsUnmanagedStruct())
            {
                throw new ArgumentException($"Can not use type {componentType.Name} as component type: not an unmanaged struct");
            }
        }

        public void CreateEntity(params Type[] componentTypes)
        {
            foreach(var componentType in componentTypes)
            {
                ThrowIfNotUnmanaged(componentType);
            }

            // Add to the list of operations
            _addEntityOperations.Add(() => CurrentTickData.CreateEntity(componentTypes));
        }
        public void CreateEntity<T1>(T1 data1)
            where T1 : struct
        {
            ThrowIfNotUnmanaged(typeof(T1));
            _addEntityOperations.Add(() => 
            { 
                var e = CurrentTickData.CreateEntity<T1>(); 
                CurrentTickData.SetComponentData(e, data1); 
                return e; 
            });
        }
        public void CreateEntity<T1, T2>(T1 data1, T2 data2)
            where T1 : struct where T2 : struct
        {
            ThrowIfNotUnmanaged(typeof(T1)); ThrowIfNotUnmanaged(typeof(T2));
            _addEntityOperations.Add(() => 
            { 
                var e = CurrentTickData.CreateEntity<T1, T2>(); 
                CurrentTickData.SetComponentData(e, data1);
                CurrentTickData.SetComponentData(e, data2);
                return e; 
            });
        }
        public void CreateEntity<T1, T2, T3>(T1 data1, T2 data2, T3 data3)
            where T1 : struct where T2 : struct where T3 : struct
        {
            ThrowIfNotUnmanaged(typeof(T1)); ThrowIfNotUnmanaged(typeof(T2)); ThrowIfNotUnmanaged(typeof(T3));
            _addEntityOperations.Add(() =>
            {
                var e = CurrentTickData.CreateEntity<T1, T2, T3>();
                CurrentTickData.SetComponentData(e, data1);
                CurrentTickData.SetComponentData(e, data2);
                CurrentTickData.SetComponentData(e, data3);
                return e;
            });
        }
        public void CreateEntity<T1, T2, T3, T4>(T1 data1, T2 data2, T3 data3, T4 data4)
            where T1 : struct where T2 : struct where T3 : struct where T4 : struct
        {
            ThrowIfNotUnmanaged(typeof(T1)); ThrowIfNotUnmanaged(typeof(T2)); ThrowIfNotUnmanaged(typeof(T3)); ThrowIfNotUnmanaged(typeof(T4));
            _addEntityOperations.Add(() =>
            {
                var e = CurrentTickData.CreateEntity<T1, T2, T3, T4>();
                CurrentTickData.SetComponentData(e, data1);
                CurrentTickData.SetComponentData(e, data2);
                CurrentTickData.SetComponentData(e, data3);
                CurrentTickData.SetComponentData(e, data4);
                return e;
            });
        }
        public void CreateEntity<T1, T2, T3, T4, T5>(T1 data1, T2 data2, T3 data3, T4 data4, T5 data5)
            where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct
        {
            ThrowIfNotUnmanaged(typeof(T1)); ThrowIfNotUnmanaged(typeof(T2)); ThrowIfNotUnmanaged(typeof(T3)); ThrowIfNotUnmanaged(typeof(T4)); ThrowIfNotUnmanaged(typeof(T5));
            _addEntityOperations.Add(() =>
            {
                var e = CurrentTickData.CreateEntity<T1, T2, T3, T4, T5>();
                CurrentTickData.SetComponentData(e, data1);
                CurrentTickData.SetComponentData(e, data2);
                CurrentTickData.SetComponentData(e, data3);
                CurrentTickData.SetComponentData(e, data4);
                CurrentTickData.SetComponentData(e, data5);
                return e;
            });
        }
        public void CreateEntity<T1, T2, T3, T4, T5, T6>(T1 data1, T2 data2, T3 data3, T4 data4, T5 data5, T6 data6)
            where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct
        {
            ThrowIfNotUnmanaged(typeof(T1)); ThrowIfNotUnmanaged(typeof(T2)); ThrowIfNotUnmanaged(typeof(T3)); ThrowIfNotUnmanaged(typeof(T4)); ThrowIfNotUnmanaged(typeof(T5)); ThrowIfNotUnmanaged(typeof(T6));
            _addEntityOperations.Add(() =>
            {
                var e = CurrentTickData.CreateEntity<T1, T2, T3, T4, T5, T6>();
                CurrentTickData.SetComponentData(e, data1);
                CurrentTickData.SetComponentData(e, data2);
                CurrentTickData.SetComponentData(e, data3);
                CurrentTickData.SetComponentData(e, data4);
                CurrentTickData.SetComponentData(e, data5);
                CurrentTickData.SetComponentData(e, data6);
                return e;
            });
        }

        public void CreateEntity<T1, T2, T3, T4, T5, T6, T7>(T1 data1, T2 data2, T3 data3, T4 data4, T5 data5, T6 data6, T7 data7)
            where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct where T7 : struct
        {
            ThrowIfNotUnmanaged(typeof(T1)); ThrowIfNotUnmanaged(typeof(T2)); ThrowIfNotUnmanaged(typeof(T3)); ThrowIfNotUnmanaged(typeof(T4)); ThrowIfNotUnmanaged(typeof(T5)); ThrowIfNotUnmanaged(typeof(T6)); ThrowIfNotUnmanaged(typeof(T7));
            _addEntityOperations.Add(() =>
            {
                var e = CurrentTickData.CreateEntity<T1, T2, T3, T4, T5, T6, T7>();
                CurrentTickData.SetComponentData(e, data1);
                CurrentTickData.SetComponentData(e, data2);
                CurrentTickData.SetComponentData(e, data3);
                CurrentTickData.SetComponentData(e, data4);
                CurrentTickData.SetComponentData(e, data5);
                CurrentTickData.SetComponentData(e, data6);
                CurrentTickData.SetComponentData(e, data7);
                return e;
            });
        }

        public void CreateEntity<T1, T2, T3, T4, T5, T6, T7, T8>(T1 data1, T2 data2, T3 data3, T4 data4, T5 data5, T6 data6, T7 data7, T8 data8)
            where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct where T7 : struct where T8 : struct
        {
            ThrowIfNotUnmanaged(typeof(T1)); ThrowIfNotUnmanaged(typeof(T2)); ThrowIfNotUnmanaged(typeof(T3)); ThrowIfNotUnmanaged(typeof(T4)); ThrowIfNotUnmanaged(typeof(T5)); ThrowIfNotUnmanaged(typeof(T6)); ThrowIfNotUnmanaged(typeof(T7)); ThrowIfNotUnmanaged(typeof(T8));
            _addEntityOperations.Add(() =>
            {
                var e = CurrentTickData.CreateEntity<T1, T2, T3, T4, T5, T6, T7, T8>();
                CurrentTickData.SetComponentData(e, data1);
                CurrentTickData.SetComponentData(e, data2);
                CurrentTickData.SetComponentData(e, data3);
                CurrentTickData.SetComponentData(e, data4);
                CurrentTickData.SetComponentData(e, data5);
                CurrentTickData.SetComponentData(e, data6);
                CurrentTickData.SetComponentData(e, data7);
                CurrentTickData.SetComponentData(e, data8);
                return e;
            });
        }

        public void RemoveEntity(Entity e)
        {
            if (!_removeEntityOperations.TryGetValue(e.ArchetypeId, out SortedDictionary<int, Entity> archetypeEntitiesToRemove))
            {
                archetypeEntitiesToRemove = new SortedDictionary<int, Entity>(new DescendingComparer<int>());
                _removeEntityOperations[e.ArchetypeId] = archetypeEntitiesToRemove;
            }
            if (!archetypeEntitiesToRemove.ContainsKey(e.Index))
            {
                archetypeEntitiesToRemove.Add(e.Index, e);
            }
        }

        public bool HasComponent<T>(Entity entity) where T : struct
            => CurrentTickData.HasComponent<T>(entity);

        public bool HasComponents(Entity entity, params Type[] componentTypes)
            => CurrentTickData.HasComponents(entity, componentTypes);

        public bool HasComponents<T1, T2>(Entity entity) 
            where T1 : struct
            where T2 : struct
            => CurrentTickData.HasComponents<T1, T2>(entity);

        public bool HasComponents<T1, T2, T3>(Entity entity)
            where T1 : struct
            where T2 : struct
            where T3 : struct
            => CurrentTickData.HasComponents<T1, T2, T3>(entity);

        public bool HasComponents<T1, T2, T3, T4>(Entity entity)
            where T1 : struct
            where T2 : struct
            where T3 : struct
            where T4 : struct
            => CurrentTickData.HasComponents<T1, T2, T3, T4>(entity);
        public bool HasComponents<T1, T2, T3, T4, T5>(Entity entity)
            where T1 : struct
            where T2 : struct
            where T3 : struct
            where T4 : struct
            where T5 : struct
            => CurrentTickData.HasComponents<T1, T2, T3, T4, T5>(entity);

        public bool HasComponents<T1, T2, T3, T4, T5, T6>(Entity entity)
            where T1 : struct
            where T2 : struct
            where T3 : struct
            where T4 : struct
            where T5 : struct
            where T6 : struct
            => CurrentTickData.HasComponents<T1, T2, T3, T4, T5, T6>(entity);


        public bool HasComponents<T1, T2, T3, T4, T5, T6, T7>(Entity entity)
            where T1 : struct
            where T2 : struct
            where T3 : struct
            where T4 : struct
            where T5 : struct
            where T6 : struct
            where T7 : struct
            => CurrentTickData.HasComponents<T1, T2, T3, T4, T5, T7>(entity);

        public bool HasComponents<T1, T2, T3, T4, T5, T6, T7, T8>(Entity entity)
            where T1 : struct
            where T2 : struct
            where T3 : struct
            where T4 : struct
            where T5 : struct
            where T6 : struct
            where T7 : struct
            where T8 : struct
            => CurrentTickData.HasComponents<T1, T2, T3, T4, T5, T7, T8>(entity);

        public CommitResult Commit()
        {
            // Execute pending operation. Todo remove/reduce allocations
            Entity[] newEntities = new Entity[_addEntityOperations.Count];
            for (int i = 0; i < _addEntityOperations.Count; i++)
            {
                newEntities[i] = _addEntityOperations[i].Invoke();
            }
            _addEntityOperations.Clear();

            for (int i = 0; i < _addArchetypeOperations.Count; i++)
            {
                AddArchetypeOperation addArchetypeOperation = _addArchetypeOperations[i];
                if (CurrentTickData.TryGetArchetype(out var _, addArchetypeOperation.ComponentTypes))
                {
                    throw new InvalidOperationException($"Failed to create archetype");
                }

                CurrentTickData.CreateArchetype(addArchetypeOperation.ComponentTypes);
            }
            _addArchetypeOperations.Clear();

            Entity[] removedEntities = new Entity[_removeEntityOperations.Sum(kvp => kvp.Value.Count())];
            int a = 0;
            foreach(KeyValuePair<int, SortedDictionary<int, Entity>> kvp in _removeEntityOperations)
            {
                int archetypeId = kvp.Key;
                SortedDictionary<int, Entity> archetypeEntitiesToRemove = kvp.Value;
                foreach(KeyValuePair<int, Entity> kvp1 in archetypeEntitiesToRemove)
                {
                    int entityIndex = kvp1.Key;
                    Entity entityToRemove = kvp1.Value;
                    CurrentTickData.RemoveEntity(entityToRemove);
                    removedEntities[a] = entityToRemove;
                    a++;
                }
                archetypeEntitiesToRemove.Clear();
            }

            TickSnapshot currentTickData = _snapshots.Last.Value;

            currentTickData.CommitResult = new CommitResult(newEntities, removedEntities);
            _snapshots.Last.Value = currentTickData;

            return currentTickData.CommitResult;
        }

        public void SetComponentData<T1>(Entity entity, T1 data)
            where T1 : struct
        {
            CurrentTickData.SetComponentData<T1>(entity, data);
        }

        public bool TryGetComponentData<T1>(Entity entity, out T1 data) where T1 : struct 
            => CurrentTickData.TryGetComponentData(entity, out data);

        public bool TryGetComponentData<T1, T2>(Entity entity, out T1 data1, out T2 data2) 
            where T1 : struct
            where T2 : struct
            => CurrentTickData.TryGetComponentData(entity, out data1, out data2);

        public bool TryGetComponentData<T1, T2, T3>(Entity entity, out T1 data1, out T2 data2, out T3 data3)
            where T1 : struct
            where T2 : struct
            where T3 : struct
            => CurrentTickData.TryGetComponentData(entity, out data1, out data2, out data3);

        public bool TryGetComponentData<T1, T2, T3, T4>(Entity entity, out T1 data1, out T2 data2, out T3 data3, out T4 data4)
            where T1 : struct
            where T2 : struct
            where T3 : struct
            where T4 : struct
            => CurrentTickData.TryGetComponentData(entity, out data1, out data2, out data3, out data4);

        public bool TryGetComponentData<T1, T2, T3, T4, T5>(Entity entity, out T1 data1, out T2 data2, out T3 data3, out T4 data4, out T5 data5)
            where T1 : struct
            where T2 : struct
            where T3 : struct
            where T4 : struct
            where T5 : struct
            => CurrentTickData.TryGetComponentData(entity, out data1, out data2, out data3, out data4, out data5);

        public bool TryGetComponentData<T1, T2, T3, T4, T5, T6>(Entity entity, out T1 data1, out T2 data2, out T3 data3, out T4 data4, out T5 data5, out T6 data6)
            where T1 : struct
            where T2 : struct
            where T3 : struct
            where T4 : struct
            where T5 : struct
            where T6 : struct
            => CurrentTickData.TryGetComponentData(entity, out data1, out data2, out data3, out data4, out data5, out data6);

        public T1 GetComponentData<T1>(Entity entity) where T1 : struct
        {
            return CurrentTickData.GetComponentData<T1>(entity);
        }

        public ArchetypeEnumerable GetArchetypesContainingAll(params Type[] componentTypes)
            => CurrentTickData.GetArchetypesContainingAll(componentTypes);

        public ArchetypeEnumerable GetArchetypesContainingAny(params Type[] componentTypes)
            => CurrentTickData.GetArchetypesContainingAny(componentTypes);

        public ComponentCollection GetArchetype(params Type[] componentTypes)
            => CurrentTickData.GetArchetype(componentTypes);

        public ComponentCollection GetArchetype(int archetypeId)
            => CurrentTickData.GetArchetype(archetypeId);

        public bool TryGetArchetype(out ComponentCollection archetype, params Type[] componentTypes)
        {
            archetype = CurrentTickData.GetArchetype(componentTypes);
            return archetype != null;
        }

        public void CaptureSnapshot()
        {
            // Get next tick snapshot from the pool or create a new one
            LinkedListNode<TickSnapshot> newSnapshotNode = GetOrCreateTickSnapshot();

            // Copy current snapshot data into the snapshot, making it newest
            TickSnapshot nextSnapshot = newSnapshotNode.Value;
            ArchetypeCollection nextSnapshotData = nextSnapshot.TickData;
            CurrentTickData.CopyTo(nextSnapshotData);
            nextSnapshot.CommitResult = new CommitResult();

            // Remove oldest snapshot from the tail and move to the head of the queue, making it newest
            _snapshots.AddLast(newSnapshotNode);
        }

        public void FreeSnapshot()
        {
            // Remove snapshot from the list and move it to the pool of free snapshots
            LinkedListNode<TickSnapshot> snapshotNode = _snapshots.First;
            if (snapshotNode.Next == null)
                return; // Never delete last snapshot

            //_snapshots.RemoveFirst();
            //_snapshootPool.AddLast(snapshotNode);
        }

        private LinkedListNode<TickSnapshot> GetOrCreateTickSnapshot()
        {
            if(_snapshootPool.First != null)
            {
                LinkedListNode<TickSnapshot> pooledSnapshot = _snapshootPool.First;
                _snapshootPool.RemoveFirst();
                return pooledSnapshot;
            }

            return new LinkedListNode<TickSnapshot>(new TickSnapshot() { TickData = new ArchetypeCollection()  });
        }

        public void Rollback(int numSnapshots)
        {
            // For the number of frames, remove last and move it back to the beginning of the list
            for(int i=0; i<numSnapshots; i++)
            {
                LinkedListNode<TickSnapshot> newestSnapshotNode = _snapshots.Last;

                if(newestSnapshotNode == null)
                    throw new ArgumentException($"Can not roll back, too many snapshots to roll back: {numSnapshots}");

                _snapshots.RemoveLast();

                // Return to the pool
                _snapshootPool.AddLast(newestSnapshotNode);
            }

            if(_snapshots.Last == null)
            {
                throw new ArgumentException($"Failed to roll back, too many snapshots to roll back: {numSnapshots}. No snapshots left!");
            }
        }

        public ArchetypeCollection GetTickData(int numTicksAgo)
        {
            LinkedListNode<TickSnapshot> currentSnapshot = _snapshots.Last;

            for(int i=0; i<numTicksAgo; i++)
            {
                currentSnapshot = currentSnapshot?.Previous;
            }

            if(currentSnapshot == null)
            {
                throw new ArgumentException($"Can not get tick data for {numTicksAgo} ticks back. Only have {_snapshots.Count()} ticks in the list");
            }

            return currentSnapshot.Value.TickData;
        }

        internal void SetTickData(ArchetypeCollection currentTickData)
        {
            currentTickData.CopyTo(CurrentTickData);
        }

        public CommitResult GetTickCommitResult(int numTicksAgo)
        {
            LinkedListNode<TickSnapshot> currentSnapshot = _snapshots.Last;

            for (int i = 0; i < numTicksAgo; i++)
            {
                currentSnapshot = currentSnapshot?.Previous;
            }

            if (currentSnapshot == null)
            {
                throw new ArgumentException($"Can not get tick data for {numTicksAgo} ticks back. Only have {_snapshots.Count()} ticks in the list");
            }

            return currentSnapshot.Value.CommitResult;
        }


        private class DescendingComparer<T> : IComparer<T> where T : IComparable<T>
        {
            public int Compare(T x, T y)
            {
                return y.CompareTo(x);
            }
        }

        struct TickSnapshot
        {
            public ArchetypeCollection TickData;
            public CommitResult CommitResult;
        }

    }
}
