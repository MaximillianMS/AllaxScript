using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Allax
{
    public struct WorkerParams
    {
        public WorkerParams(IEngine Engine, int MaxThreads=1)
        {
            this.MaxThreads = MaxThreads;
            this.Engine = Engine;
        }
        public IEngine Engine;
        public int MaxThreads;
    }
    public struct WorkerThreadParams
    {
        public Task T;
        public WorkerThreadState State;
        public WorkerThreadParams(Task t)
        {
            T = t;
            State = WorkerThreadState.Loaded;
        }
    }
    public enum WorkerThreadState {Free, Loaded, Started, Paused, Stopped }
}
