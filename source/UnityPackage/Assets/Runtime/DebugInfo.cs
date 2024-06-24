using System;

namespace Fenrir.ECS
{
    public struct DebugInfo
    {
        public int MtuBytes;
        public int NetRttMs;
        public TimeSpan SimRtt;
        public TimeSpan SimJitter;
        public TimeSpan SimLastSyncTime;
        public TimeSpan SimNextSyncTime;
        public TimeSpan SimClockOffset;
        public int CurrentTick;
        public int ConfirmedTick;
    }
}
