using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Allax
{
    public struct WorkerParams
    {
        public WorkerParams(int MaxThreads, TaskerParams TaskerParams, TaskFinishedHandler TaskFinishedFunc)
        {
            this.TaskerParams = TaskerParams;
            this.MaxThreads = MaxThreads;
            this.TaskFinishedFunc = TaskFinishedFunc;
        }
        public TaskFinishedHandler TaskFinishedFunc;
        public int MaxThreads;
        public TaskerParams TaskerParams;
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
