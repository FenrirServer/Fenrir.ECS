using Fenrir.Multiplayer;
using System;
using System.Collections.Concurrent;

namespace Fenrir.ECS
{
    internal abstract class EntityViewObserver : ISimulationObserver
    {
        /// <summary>
        /// Entity view
        /// </summary>
        private readonly IEntityView _entityView;

        /// <summary>
        /// Moba client
        /// </summary>
        protected readonly ISimulationClient _client;

        /// <summary>
        /// Logger
        /// </summary>
        protected readonly ILogger _logger;

        /// <summary>
        /// Type collection
        /// </summary>
        protected TypeCollection _typeCollection;

        private World World => _client.Simulation.ECSWorld;

        private int CurrentTick => _client.Simulation.CurrentTick;
        private int LastConfirmedTick => _client.LastConfirmedTick;

        protected Action<ComponentCollection, EntityViewOperationData> _addCallback;

        internal EntityViewObserver(IEntityView entityView, ISimulationClient simulationClient, ILogger logger)
        {
            _entityView = entityView;
            _client = simulationClient;
            _logger = logger;
        }

        protected abstract void InvokeCallbacks();

        public void Update()
        {
            try
            {
                InvokeCallbacks();
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                _logger.Error($"Error during OnEntityCreated. See stack trace above.");
            }
        }

        private void HandleSimulationCommitResult(CommitResult commitResult)
        {
            // Create new entity views
            if (commitResult.AddedEntities != null)
            {
                foreach (Entity newEntity in commitResult.AddedEntities)
                {
                    var archetype = World.GetArchetype(newEntity.ArchetypeId);
                    if (archetype.HasComponents(_typeCollection.Types))
                        _addCallback(archetype, new EntityViewOperationData(new EntityCreateData(newEntity, CurrentTick)));
                }
            }

            // Destroy entity views
            if (commitResult.RemovedEntities != null)
            {
                foreach (Entity removedEntity in commitResult.RemovedEntities)
                {
                    var archetype = World.GetArchetype(removedEntity.ArchetypeId);
                    if (archetype.HasComponents(_typeCollection.Types))
                        _addCallback(archetype, new EntityViewOperationData(new EntityDestroyData(removedEntity, CurrentTick)));
                }
            }
        }

        private void HandleSimulationConfirmedCommitResult(ArchetypeCollection tickData, CommitResult commitResult)
        {
            // Create new entity views
            if (commitResult.AddedEntities != null)
            {
                foreach (Entity newEntity in commitResult.AddedEntities)
                {
                    if (tickData.HasComponents(newEntity, _typeCollection.Types))
                    {
                        var archetype = tickData.GetArchetype(newEntity.ArchetypeId);
                        _addCallback(archetype, new EntityViewOperationData(new EntityConfirmedCreateData(newEntity, LastConfirmedTick)));
                    }
                }
            }

            // Destroy entity views
            if (commitResult.RemovedEntities != null)
            {
                foreach (Entity removedEntity in commitResult.RemovedEntities)
                {
                    if (tickData.HasComponents(removedEntity, _typeCollection.Types))
                    {
                        var archetype = tickData.GetArchetype(removedEntity.ArchetypeId);
                        _addCallback(archetype, new EntityViewOperationData(new EntityConfirmedDestroyData(removedEntity, LastConfirmedTick)));
                    }
                }
            }
        }
            
        void ISimulationObserver.OnSimulationTick(CommitResult commitResult, bool didRollBack)
        {
            HandleSimulationCommitResult(commitResult);

            ArchetypeEnumerable archetypes = World.GetArchetypesContainingAll(_typeCollection.Types);
            foreach (ComponentCollection archetype in archetypes)
            {
                EntityIterator entities = archetype.GetEntities();
                for (int i = 0; i < entities.NumEntities; i++)
                {
                    Entity entity = entities[i];
                    _addCallback(archetype, new EntityViewOperationData(new EntityTickData(entity, CurrentTick, didRollBack)));
                }
            }
        }

        void ISimulationObserver.OnSimulationConfirmedTick(int numTick, ArchetypeCollection tickData, CommitResult commitResult)
        {
            HandleSimulationConfirmedCommitResult(tickData, commitResult);

            ArchetypeEnumerable archetypes = tickData.GetArchetypesContainingAll(_typeCollection.Types);
            foreach (ComponentCollection archetype in archetypes)
            {
                EntityIterator entities = archetype.GetEntities();
                for (int i = 0; i < entities.NumEntities; i++)
                {
                    Entity entity = entities[i];
                    _addCallback(archetype, new EntityViewOperationData(new EntityConfirmedTickData(entity, numTick)));
                }
            }
        }

        void ISimulationObserver.OnSimulationRollback(int numTicks)
        {
            ArchetypeEnumerable archetypes = World.GetArchetypesContainingAll(_typeCollection.Types);
            foreach (ComponentCollection archetype in archetypes)
            {
                EntityIterator entities = archetype.GetEntities();
                for (int i = 0; i < entities.NumEntities; i++)
                {
                    Entity entity = entities[i];
                    _addCallback(archetype, new EntityViewOperationData(new EntityRollbackData(entity, CurrentTick, numTicks)));
                }
            }
        }
    }


    internal class EntityViewObserver<T1> : EntityViewObserver
        where T1 : struct
    {
        IEntityView<T1> _entityView;
        ConcurrentQueue<EntityViewOperationDataWrapper<T1>> _operationQueue = new ConcurrentQueue<EntityViewOperationDataWrapper<T1>>();

        public EntityViewObserver(IEntityView<T1> entityView, ISimulationClient simulationClient, ILogger logger) : base(entityView, simulationClient, logger)
        {
            _entityView = entityView;
            _typeCollection = TypeCollection.Create<T1>();
            _addCallback = (archetype, od) => _operationQueue.Enqueue(new EntityViewOperationDataWrapper<T1>()
            {
                OperationData = od,
                Data1 = archetype.GetComponentData<T1>(od.Entity)
            });
        }
        protected override void InvokeCallbacks()
        {
            while (_operationQueue.TryDequeue(out var wrapper))
                if (wrapper.OperationData.Operation == EntityViewOperation.Create)
                    _entityView.OnEntityCreated(wrapper.OperationData.CreateData, wrapper.Data1);
                else if (wrapper.OperationData.Operation == EntityViewOperation.ConfirmedCreate)
                    _entityView.OnEntityConfirmedCreated(wrapper.OperationData.ConfirmedCreateData, wrapper.Data1);
                else if (wrapper.OperationData.Operation == EntityViewOperation.Tick)
                    _entityView.OnEntityTick(wrapper.OperationData.TickData, wrapper.Data1);
                else if (wrapper.OperationData.Operation == EntityViewOperation.ConfirmedTick)
                    _entityView.OnEntityConfirmedTick(wrapper.OperationData.ConfirmedTickData, wrapper.Data1);
                else if (wrapper.OperationData.Operation == EntityViewOperation.Rollback)
                    _entityView.OnEntityRollback(wrapper.OperationData.RollbackData);
                else if (wrapper.OperationData.Operation == EntityViewOperation.Destroy)
                    _entityView.OnEntityDestroyed(wrapper.OperationData.DestroyData);
                else if (wrapper.OperationData.Operation == EntityViewOperation.ConfirmedDestroy)
                    _entityView.OnEntityConfirmedDestroyed(wrapper.OperationData.ConfirmedDestroyData);
        }
    }

    internal class EntityViewObserver<T1, T2> : EntityViewObserver
        where T1 : struct where T2 : struct
    {
        IEntityView<T1, T2> _entityView;
        ConcurrentQueue<EntityViewOperationDataWrapper<T1, T2>> _operationQueue = new ConcurrentQueue<EntityViewOperationDataWrapper<T1, T2>>();

        public EntityViewObserver(IEntityView<T1, T2> entityView, ISimulationClient simulationClient, ILogger logger) : base(entityView, simulationClient, logger)
        {
            _entityView = entityView;
            _typeCollection = TypeCollection.Create<T1, T2>();
            _addCallback = (archetype, od) => _operationQueue.Enqueue(new EntityViewOperationDataWrapper<T1, T2>()
            {
                OperationData = od,
                Data1 = archetype.GetComponentData<T1>(od.Entity),
                Data2 = archetype.GetComponentData<T2>(od.Entity)
            });
        }
        protected override void InvokeCallbacks()
        {
            while (_operationQueue.TryDequeue(out var wrapper))
                if (wrapper.OperationData.Operation == EntityViewOperation.Create)
                    _entityView.OnEntityCreated(wrapper.OperationData.CreateData, wrapper.Data1, wrapper.Data2);
                else if (wrapper.OperationData.Operation == EntityViewOperation.ConfirmedCreate)
                    _entityView.OnEntityConfirmedCreated(wrapper.OperationData.ConfirmedCreateData, wrapper.Data1, wrapper.Data2);
                else if (wrapper.OperationData.Operation == EntityViewOperation.Tick)
                    _entityView.OnEntityTick(wrapper.OperationData.TickData, wrapper.Data1, wrapper.Data2);
                else if (wrapper.OperationData.Operation == EntityViewOperation.ConfirmedTick)
                    _entityView.OnEntityConfirmedTick(wrapper.OperationData.ConfirmedTickData, wrapper.Data1, wrapper.Data2);
                else if (wrapper.OperationData.Operation == EntityViewOperation.Rollback)
                    _entityView.OnEntityRollback(wrapper.OperationData.RollbackData);
                else if (wrapper.OperationData.Operation == EntityViewOperation.Destroy)
                    _entityView.OnEntityDestroyed(wrapper.OperationData.DestroyData);
                else if (wrapper.OperationData.Operation == EntityViewOperation.ConfirmedDestroy)
                    _entityView.OnEntityConfirmedDestroyed(wrapper.OperationData.ConfirmedDestroyData);
        }
    }

    internal class EntityViewObserver<T1, T2, T3> : EntityViewObserver
        where T1 : struct
        where T2 : struct
        where T3 : struct
    {
        IEntityView<T1, T2, T3> _entityView;
        ConcurrentQueue<EntityViewOperationDataWrapper<T1, T2, T3>> _operationQueue = new ConcurrentQueue<EntityViewOperationDataWrapper<T1, T2, T3>>();

        public EntityViewObserver(IEntityView<T1, T2, T3> entityView, ISimulationClient simulationClient, ILogger logger)
            : base(entityView, simulationClient, logger)
        {
            _entityView = entityView;
            _typeCollection = TypeCollection.Create<T1, T2, T3>();
            _addCallback = (archetype, od) => _operationQueue.Enqueue(new EntityViewOperationDataWrapper<T1, T2, T3>()
            {
                OperationData = od,
                Data1 = archetype.GetComponentData<T1>(od.Entity),
                Data2 = archetype.GetComponentData<T2>(od.Entity),
                Data3 = archetype.GetComponentData<T3>(od.Entity)
            });
        }
        protected override void InvokeCallbacks()
        {
            while (_operationQueue.TryDequeue(out var wrapper))
                if (wrapper.OperationData.Operation == EntityViewOperation.Create)
                    _entityView.OnEntityCreated(wrapper.OperationData.CreateData, wrapper.Data1, wrapper.Data2, wrapper.Data3);
                else if (wrapper.OperationData.Operation == EntityViewOperation.ConfirmedCreate)
                    _entityView.OnEntityConfirmedCreated(wrapper.OperationData.ConfirmedCreateData, wrapper.Data1, wrapper.Data2, wrapper.Data3);
                else if (wrapper.OperationData.Operation == EntityViewOperation.Tick)
                    _entityView.OnEntityTick(wrapper.OperationData.TickData, wrapper.Data1, wrapper.Data2, wrapper.Data3);
                else if (wrapper.OperationData.Operation == EntityViewOperation.ConfirmedTick)
                    _entityView.OnEntityConfirmedTick(wrapper.OperationData.ConfirmedTickData, wrapper.Data1, wrapper.Data2, wrapper.Data3);
                else if (wrapper.OperationData.Operation == EntityViewOperation.Rollback)
                    _entityView.OnEntityRollback(wrapper.OperationData.RollbackData);
                else if (wrapper.OperationData.Operation == EntityViewOperation.Destroy)
                    _entityView.OnEntityDestroyed(wrapper.OperationData.DestroyData);
                else if (wrapper.OperationData.Operation == EntityViewOperation.ConfirmedDestroy)
                    _entityView.OnEntityConfirmedDestroyed(wrapper.OperationData.ConfirmedDestroyData);
        }
    }

    internal class EntityViewObserver<T1, T2, T3, T4> : EntityViewObserver
        where T1 : struct
        where T2 : struct
        where T3 : struct
        where T4 : struct
    {
        IEntityView<T1, T2, T3, T4> _entityView;
        ConcurrentQueue<EntityViewOperationDataWrapper<T1, T2, T3, T4>> _operationQueue = new ConcurrentQueue<EntityViewOperationDataWrapper<T1, T2, T3, T4>>();

        public EntityViewObserver(IEntityView<T1, T2, T3, T4> entityView, ISimulationClient simulationClient, ILogger logger)
            : base(entityView, simulationClient, logger)
        {
            _entityView = entityView;
            _typeCollection = TypeCollection.Create<T1, T2, T3, T4>();
            _addCallback = (archetype, od) => _operationQueue.Enqueue(new EntityViewOperationDataWrapper<T1, T2, T3, T4>()
            {
                OperationData = od,
                Data1 = archetype.GetComponentData<T1>(od.Entity),
                Data2 = archetype.GetComponentData<T2>(od.Entity),
                Data3 = archetype.GetComponentData<T3>(od.Entity),
                Data4 = archetype.GetComponentData<T4>(od.Entity)
            });
        }
        protected override void InvokeCallbacks()
        {
            while (_operationQueue.TryDequeue(out var wrapper))
                if (wrapper.OperationData.Operation == EntityViewOperation.Create)
                    _entityView.OnEntityCreated(wrapper.OperationData.CreateData, wrapper.Data1, wrapper.Data2, wrapper.Data3, wrapper.Data4);
                else if (wrapper.OperationData.Operation == EntityViewOperation.ConfirmedCreate)
                    _entityView.OnEntityConfirmedCreated(wrapper.OperationData.ConfirmedCreateData, wrapper.Data1, wrapper.Data2, wrapper.Data3, wrapper.Data4);
                else if (wrapper.OperationData.Operation == EntityViewOperation.Tick)
                    _entityView.OnEntityTick(wrapper.OperationData.TickData, wrapper.Data1, wrapper.Data2, wrapper.Data3, wrapper.Data4);
                else if (wrapper.OperationData.Operation == EntityViewOperation.ConfirmedTick)
                    _entityView.OnEntityConfirmedTick(wrapper.OperationData.ConfirmedTickData, wrapper.Data1, wrapper.Data2, wrapper.Data3, wrapper.Data4);
                else if (wrapper.OperationData.Operation == EntityViewOperation.Rollback)
                    _entityView.OnEntityRollback(wrapper.OperationData.RollbackData);
                else if (wrapper.OperationData.Operation == EntityViewOperation.Destroy)
                    _entityView.OnEntityDestroyed(wrapper.OperationData.DestroyData);
                else if (wrapper.OperationData.Operation == EntityViewOperation.ConfirmedDestroy)
                    _entityView.OnEntityConfirmedDestroyed(wrapper.OperationData.ConfirmedDestroyData);
        }
    }

    internal class EntityViewObserver<T1, T2, T3, T4, T5> : EntityViewObserver
        where T1 : struct
        where T2 : struct
        where T3 : struct
        where T4 : struct
        where T5 : struct
    {
        IEntityView<T1, T2, T3, T4, T5> _entityView;
        ConcurrentQueue<EntityViewOperationDataWrapper<T1, T2, T3, T4, T5>> _operationQueue = new ConcurrentQueue<EntityViewOperationDataWrapper<T1, T2, T3, T4, T5>>();

        public EntityViewObserver(IEntityView<T1, T2, T3, T4, T5> entityView, ISimulationClient simulationClient, ILogger logger)
            : base(entityView, simulationClient, logger)
        {
            _entityView = entityView;
            _typeCollection = TypeCollection.Create<T1, T2, T3, T4, T5>();
            _addCallback = (archetype, od) => _operationQueue.Enqueue(new EntityViewOperationDataWrapper<T1, T2, T3, T4, T5>()
            {
                OperationData = od,
                Data1 = archetype.GetComponentData<T1>(od.Entity),
                Data2 = archetype.GetComponentData<T2>(od.Entity),
                Data3 = archetype.GetComponentData<T3>(od.Entity),
                Data4 = archetype.GetComponentData<T4>(od.Entity),
                Data5 = archetype.GetComponentData<T5>(od.Entity)
            });
        }
        protected override void InvokeCallbacks()
        {
            while (_operationQueue.TryDequeue(out var wrapper))
                if (wrapper.OperationData.Operation == EntityViewOperation.Create)
                    _entityView.OnEntityCreated(wrapper.OperationData.CreateData, wrapper.Data1, wrapper.Data2, wrapper.Data3, wrapper.Data4, wrapper.Data5);
                else if (wrapper.OperationData.Operation == EntityViewOperation.ConfirmedCreate)
                    _entityView.OnEntityConfirmedCreated(wrapper.OperationData.ConfirmedCreateData, wrapper.Data1, wrapper.Data2, wrapper.Data3, wrapper.Data4, wrapper.Data5);
                else if (wrapper.OperationData.Operation == EntityViewOperation.Tick)
                    _entityView.OnEntityTick(wrapper.OperationData.TickData, wrapper.Data1, wrapper.Data2, wrapper.Data3, wrapper.Data4, wrapper.Data5);
                else if (wrapper.OperationData.Operation == EntityViewOperation.ConfirmedTick)
                    _entityView.OnEntityConfirmedTick(wrapper.OperationData.ConfirmedTickData, wrapper.Data1, wrapper.Data2, wrapper.Data3, wrapper.Data4, wrapper.Data5);
                else if (wrapper.OperationData.Operation == EntityViewOperation.Rollback)
                    _entityView.OnEntityRollback(wrapper.OperationData.RollbackData);
                else if (wrapper.OperationData.Operation == EntityViewOperation.Destroy)
                    _entityView.OnEntityDestroyed(wrapper.OperationData.DestroyData);
                else if (wrapper.OperationData.Operation == EntityViewOperation.ConfirmedDestroy)
                    _entityView.OnEntityConfirmedDestroyed(wrapper.OperationData.ConfirmedDestroyData);
        }
    }

    internal class EntityViewObserver<T1, T2, T3, T4, T5, T6> : EntityViewObserver
        where T1 : struct
        where T2 : struct
        where T3 : struct
        where T4 : struct
        where T5 : struct
        where T6 : struct
    {
        IEntityView<T1, T2, T3, T4, T5, T6> _entityView;
        ConcurrentQueue<EntityViewOperationDataWrapper<T1, T2, T3, T4, T5, T6>> _operationQueue = new ConcurrentQueue<EntityViewOperationDataWrapper<T1, T2, T3, T4, T5, T6>>();

        public EntityViewObserver(IEntityView<T1, T2, T3, T4, T5, T6> entityView, ISimulationClient simulationClient, ILogger logger)
            : base(entityView, simulationClient, logger)
        {
            _entityView = entityView;
            _typeCollection = TypeCollection.Create<T1, T2, T3, T4, T5, T6>();
            _addCallback = (archetype, od) => _operationQueue.Enqueue(new EntityViewOperationDataWrapper<T1, T2, T3, T4, T5, T6>()
            {
                OperationData = od,
                Data1 = archetype.GetComponentData<T1>(od.Entity),
                Data2 = archetype.GetComponentData<T2>(od.Entity),
                Data3 = archetype.GetComponentData<T3>(od.Entity),
                Data4 = archetype.GetComponentData<T4>(od.Entity),
                Data5 = archetype.GetComponentData<T5>(od.Entity),
                Data6 = archetype.GetComponentData<T6>(od.Entity)
            });
        }
        protected override void InvokeCallbacks()
        {
            while (_operationQueue.TryDequeue(out var wrapper))
                if (wrapper.OperationData.Operation == EntityViewOperation.Create)
                    _entityView.OnEntityCreated(wrapper.OperationData.CreateData, wrapper.Data1, wrapper.Data2, wrapper.Data3, wrapper.Data4, wrapper.Data5, wrapper.Data6);
                else if (wrapper.OperationData.Operation == EntityViewOperation.ConfirmedCreate)
                    _entityView.OnEntityConfirmedCreated(wrapper.OperationData.ConfirmedCreateData, wrapper.Data1, wrapper.Data2, wrapper.Data3, wrapper.Data4, wrapper.Data5, wrapper.Data6);
                else if (wrapper.OperationData.Operation == EntityViewOperation.Tick)
                    _entityView.OnEntityTick(wrapper.OperationData.TickData, wrapper.Data1, wrapper.Data2, wrapper.Data3, wrapper.Data4, wrapper.Data5, wrapper.Data6);
                else if (wrapper.OperationData.Operation == EntityViewOperation.ConfirmedTick)
                    _entityView.OnEntityConfirmedTick(wrapper.OperationData.ConfirmedTickData, wrapper.Data1, wrapper.Data2, wrapper.Data3, wrapper.Data4, wrapper.Data5, wrapper.Data6);
                else if (wrapper.OperationData.Operation == EntityViewOperation.Rollback)
                    _entityView.OnEntityRollback(wrapper.OperationData.RollbackData);
                else if (wrapper.OperationData.Operation == EntityViewOperation.Destroy)
                    _entityView.OnEntityDestroyed(wrapper.OperationData.DestroyData);
                else if (wrapper.OperationData.Operation == EntityViewOperation.ConfirmedDestroy)
                    _entityView.OnEntityConfirmedDestroyed(wrapper.OperationData.ConfirmedDestroyData);
        }
    }

    internal class EntityViewObserver<T1, T2, T3, T4, T5, T6, T7> : EntityViewObserver
        where T1 : struct
        where T2 : struct
        where T3 : struct
        where T4 : struct
        where T5 : struct
        where T6 : struct
        where T7 : struct
    {
        private IEntityView<T1, T2, T3, T4, T5, T6, T7> _entityView;
        ConcurrentQueue<EntityViewOperationDataWrapper<T1, T2, T3, T4, T5, T6, T7>> _operationQueue = new ConcurrentQueue<EntityViewOperationDataWrapper<T1, T2, T3, T4, T5, T6, T7>>();

        public EntityViewObserver(IEntityView<T1, T2, T3, T4, T5, T6, T7> entityView, ISimulationClient simulationClient, ILogger logger)
            : base(entityView, simulationClient, logger)
        {
            _entityView = entityView;
            _typeCollection = TypeCollection.Create<T1, T2, T3, T4, T5, T6, T7>();
            _addCallback = (archetype, od) => _operationQueue.Enqueue(new EntityViewOperationDataWrapper<T1, T2, T3, T4, T5, T6, T7>()
            {
                OperationData = od,
                Data1 = archetype.GetComponentData<T1>(od.Entity),
                Data2 = archetype.GetComponentData<T2>(od.Entity),
                Data3 = archetype.GetComponentData<T3>(od.Entity),
                Data4 = archetype.GetComponentData<T4>(od.Entity),
                Data5 = archetype.GetComponentData<T5>(od.Entity),
                Data6 = archetype.GetComponentData<T6>(od.Entity),
                Data7 = archetype.GetComponentData<T7>(od.Entity)
            });
        }

        protected override void InvokeCallbacks()
        {
            while (_operationQueue.TryDequeue(out var wrapper))
                if (wrapper.OperationData.Operation == EntityViewOperation.Create)
                    _entityView.OnEntityCreated(wrapper.OperationData.CreateData, wrapper.Data1, wrapper.Data2, wrapper.Data3, wrapper.Data4, wrapper.Data5, wrapper.Data6, wrapper.Data7);
                else if (wrapper.OperationData.Operation == EntityViewOperation.ConfirmedCreate)
                    _entityView.OnEntityConfirmedCreated(wrapper.OperationData.ConfirmedCreateData, wrapper.Data1, wrapper.Data2, wrapper.Data3, wrapper.Data4, wrapper.Data5, wrapper.Data6, wrapper.Data7);
                else if (wrapper.OperationData.Operation == EntityViewOperation.Tick)
                    _entityView.OnEntityTick(wrapper.OperationData.TickData, wrapper.Data1, wrapper.Data2, wrapper.Data3, wrapper.Data4, wrapper.Data5, wrapper.Data6, wrapper.Data7);
                else if (wrapper.OperationData.Operation == EntityViewOperation.ConfirmedTick)
                    _entityView.OnEntityConfirmedTick(wrapper.OperationData.ConfirmedTickData, wrapper.Data1, wrapper.Data2, wrapper.Data3, wrapper.Data4, wrapper.Data5, wrapper.Data6, wrapper.Data7);
                else if (wrapper.OperationData.Operation == EntityViewOperation.Rollback)
                    _entityView.OnEntityRollback(wrapper.OperationData.RollbackData);
                else if (wrapper.OperationData.Operation == EntityViewOperation.Destroy)
                    _entityView.OnEntityDestroyed(wrapper.OperationData.DestroyData);
                else if (wrapper.OperationData.Operation == EntityViewOperation.ConfirmedDestroy)
                    _entityView.OnEntityConfirmedDestroyed(wrapper.OperationData.ConfirmedDestroyData);
        }
    }


    internal class EntityViewObserver<T1, T2, T3, T4, T5, T6, T7, T8> : EntityViewObserver
        where T1 : struct
        where T2 : struct
        where T3 : struct
        where T4 : struct
        where T5 : struct
        where T6 : struct
        where T7 : struct
        where T8 : struct
    {
        private IEntityView<T1, T2, T3, T4, T5, T6, T7, T8> _entityView;
        ConcurrentQueue<EntityViewOperationDataWrapper<T1, T2, T3, T4, T5, T6, T7, T8>> _operationQueue = new ConcurrentQueue<EntityViewOperationDataWrapper<T1, T2, T3, T4, T5, T6, T7, T8>>();

        public EntityViewObserver(IEntityView<T1, T2, T3, T4, T5, T6, T7, T8> entityView, ISimulationClient simulationClient, ILogger logger)
            : base(entityView, simulationClient, logger)
        {
            _entityView = entityView;
            _typeCollection = TypeCollection.Create<T1, T2, T3, T4, T5, T6, T7, T8>();
            _addCallback = (archetype, od) => _operationQueue.Enqueue(new EntityViewOperationDataWrapper<T1, T2, T3, T4, T5, T6, T7, T8>()
            {
                OperationData = od,
                Data1 = archetype.GetComponentData<T1>(od.Entity),
                Data2 = archetype.GetComponentData<T2>(od.Entity),
                Data3 = archetype.GetComponentData<T3>(od.Entity),
                Data4 = archetype.GetComponentData<T4>(od.Entity),
                Data5 = archetype.GetComponentData<T5>(od.Entity),
                Data6 = archetype.GetComponentData<T6>(od.Entity),
                Data7 = archetype.GetComponentData<T7>(od.Entity),
                Data8 = archetype.GetComponentData<T8>(od.Entity)
            });
        }
        protected override void InvokeCallbacks()
        {
            while (_operationQueue.TryDequeue(out var wrapper))
                if (wrapper.OperationData.Operation == EntityViewOperation.Create)
                    _entityView.OnEntityCreated(wrapper.OperationData.CreateData, wrapper.Data1, wrapper.Data2, wrapper.Data3, wrapper.Data4, wrapper.Data5, wrapper.Data6, wrapper.Data7, wrapper.Data8);
                else if (wrapper.OperationData.Operation == EntityViewOperation.ConfirmedCreate)
                    _entityView.OnEntityConfirmedCreated(wrapper.OperationData.ConfirmedCreateData, wrapper.Data1, wrapper.Data2, wrapper.Data3, wrapper.Data4, wrapper.Data5, wrapper.Data6, wrapper.Data7, wrapper.Data8);
                else if (wrapper.OperationData.Operation == EntityViewOperation.Tick)
                    _entityView.OnEntityTick(wrapper.OperationData.TickData, wrapper.Data1, wrapper.Data2, wrapper.Data3, wrapper.Data4, wrapper.Data5, wrapper.Data6, wrapper.Data7, wrapper.Data8);
                else if (wrapper.OperationData.Operation == EntityViewOperation.ConfirmedTick)
                    _entityView.OnEntityConfirmedTick(wrapper.OperationData.ConfirmedTickData, wrapper.Data1, wrapper.Data2, wrapper.Data3, wrapper.Data4, wrapper.Data5, wrapper.Data6, wrapper.Data7, wrapper.Data8);
                else if (wrapper.OperationData.Operation == EntityViewOperation.Rollback)
                    _entityView.OnEntityRollback(wrapper.OperationData.RollbackData);
                else if (wrapper.OperationData.Operation == EntityViewOperation.Destroy)
                    _entityView.OnEntityDestroyed(wrapper.OperationData.DestroyData);
                else if (wrapper.OperationData.Operation == EntityViewOperation.ConfirmedDestroy)
                    _entityView.OnEntityConfirmedDestroyed(wrapper.OperationData.ConfirmedDestroyData);
        }
    }
}
