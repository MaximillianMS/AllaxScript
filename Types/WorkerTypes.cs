﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Allax
{
    public struct WorkerParams
    {
        public WorkerParams(IEngine Engine, int MaxThreads=1, bool ASync=true)
        {
            this.MaxThreads = MaxThreads;
            this.Engine = Engine;
            this.ASync = ASync;
        }
        public IEngine Engine;
        public int MaxThreads;
        public bool ASync;
    }
    public struct WorkerThreadParams
    {
        public ITask T;
        public WorkerThreadState State;
        public WorkerThreadParams(ITask t)
        {
            T = t;
            State = WorkerThreadState.Loaded;
        }
    }
    public enum WorkerThreadState {Free, Loaded, Started, Paused, Stopped, Finished }
}
