using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Allax
{
    public interface IWorkerThread
    {
        void Init(WorkerThreadParams Params);
        void Start();
        void Suspend();
        void Abort();
        void Resume();
        WorkerThreadState GetState();
    }
    public interface IWorker
    {
        void Init(WorkerParams Params);
        bool InitThread(IWorkerThread Thread);
        void Run();
        void Pause();
        void Resume();
        void AsyncRun();
    }
}
