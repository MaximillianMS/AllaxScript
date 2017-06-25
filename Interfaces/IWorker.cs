using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Allax
{
    public delegate void JOBSDONEHANDLER(IWorkerThread Thread);
    public interface IWorkerThread:IDisposable
    {
        event JOBSDONEHANDLER JOBSDONE;
        void Init(WorkerThreadParams Params);
        void Start();
        void Suspend();
        void Abort();
        void Resume();
        ITask GetCurrentTask();
        WorkerThreadState GetState();
        void SetState(WorkerThreadState State);
    }
    public interface IWorker:IDisposable
    {
        event TASKHANDLER TASKDONE;
        event TASKHANDLER ALLTASKSDONE;
        void Init(WorkerParams Params);
        void AddTasks(int count);
        void Run();
        void Pause();
        void Resume();
        void AsyncRun();
    }
}
