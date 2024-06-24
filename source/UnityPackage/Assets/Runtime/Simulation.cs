using Fenrir.Multiplayer;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Fenrir.ECS
{
    public class Simulation
    {
        public int TickRate { get; set; } = 60;

        private readonly World _ecsWorld;
        private readonly Clock _clock;
        private readonly ILogger _logger;

        public int CurrentTick = 0;

        private List<ISystem> _systems = new List<ISystem>();

        public TimeSpan TimePerTick => TimeSpan.FromTicks((long)(1000d / TickRate * TimeSpan.TicksPerMillisecond));

        public TimeSpan CurrentTickTime => TimeSpan.FromTicks((long)(CurrentTick * TimePerTick.TotalMilliseconds * TimeSpan.TicksPerMillisecond));

        public TimeSpan NextTickTime => TimeSpan.FromTicks((long)((CurrentTick+1) * TimePerTick.TotalMilliseconds * TimeSpan.TicksPerMillisecond));

        public World ECSWorld => _ecsWorld;

        public Clock Clock => _clock;


        public Simulation(Clock clock, ILogger logger)
        {
            _ecsWorld = new World();
            _clock = clock;
            _logger = logger;

            if (!Stopwatch.IsHighResolution)
            {
                _logger.Warning($"{nameof(Stopwatch.IsHighResolution)} is false. The simulation clock will not work properly on this system.");
            }
        }

        public void AddSystem(ISystem system)
        {
            _systems.Add(system);
        }

        public void RemoveSystem(ISystem system)
        {
            _systems.Remove(system);
        }

        public void CaptureSnapshot()
        {
            _ecsWorld.CaptureSnapshot();
        }

        public void FreeSnapshot()
        {
            _ecsWorld.FreeSnapshot();
        }

        public ArchetypeCollection GetCurrentTickData()
        {
            return _ecsWorld.CurrentTickData;
        }

        public ArchetypeCollection GetTickData(int numTicksAgo)
        {
            return _ecsWorld.GetTickData(numTicksAgo);
        }

        public CommitResult GetTickCommitResult(int numTicksAgo)
        {
            return _ecsWorld.GetTickCommitResult(numTicksAgo);
        }

        public CommitResult Commit()
        {
            return _ecsWorld.Commit();
        }

        public void TickSystems()
        {
            foreach(var system in _systems) 
            {
                try
                {
                    system.Tick();
                }
                catch(Exception e)
                {
                    _logger.Error(e.ToString());
                    _logger.Error($"Error during system tick {system.GetType().Name}, see log above");
                }
            }

        }

        public void Rollback(int numTicks)
        {
            _ecsWorld.Rollback(numTicks);
            CurrentTick -= numTicks;
        }
    }
}
