using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Allax
{
    public interface IWorkerThread
    {
        void Init(WorkerThreadParams Params);
        void StartThread();
    }
    public class WorkerThread : IWorkerThread
    {
        WorkerThreadParams Params;
        public void Init(WorkerThreadParams Params)
        {
            this.Params = Params;
        }
        public void StartThread()
        {
            Params.T.Exec();
        }
    }
    public interface IWorker
    {
        void Init(WorkerParams Params);
        void Run();
        void AsyncRun();
    }
    public class Worker:IWorker
    {
        WorkerParams Params;
        ITasker Tasker;
        ConcurrentQueue<Task> TaskQueue;
        List<IWorkerThread> Threads;
        public Worker(WorkerParams Params)
        {
            Init(Params);
            Tasker = new Tasker(new TaskerParams(Params.Net));
        }
        public void Init(WorkerParams Params)
        {
            this.Params = Params;
        }
        public void Run()
        {
            throw new NotImplementedException();
            Threads = new List<IWorkerThread>();
            for(int i=0;i<Params.MaxThreads;i++)
            {
                Threads.Add(new WorkerThread());
            }
            TaskQueue=Tasker.GetTasks(Params.MaxThreads);
        }
        public void AsyncRun()
        {
            throw new NotFiniteNumberException();
        }
    }
}
