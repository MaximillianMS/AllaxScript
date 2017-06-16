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
        Task GetCurrentTask();
        WorkerThreadState GetState();
    }
    public interface IWorker:IDisposable
    {
        event TASKDONEHANDLER TASKDONE;
        void Init(WorkerParams Params);
        bool InitThread(IWorkerThread Thread);
        void Run();
        void Pause();
        void Resume();
        void AsyncRun();
    }
}
