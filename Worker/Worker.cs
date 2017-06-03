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
        void Start();
        void Suspend();
        void Abort();
        WorkerThreadState GetState();
    }
    public class WorkerThread : IWorkerThread
    {
        WorkerThreadParams Params;
        public int ID;
        System.Threading.Thread Tr;
        public WorkerThread(WorkerThreadParams Params)
        {
            Init(Params);
        }

        public void Abort()
        {
            if(Tr!=null)
            {
                if(Tr.IsAlive)
                {
                    try
                    {
                        Tr.Abort();
                    }
                    catch (Exception e)
                    {
                        Logger.UltraLogger.Instance.AddToLog(String.Format("Worker: Thread {0}. Exception message: \"{1}\"", Tr.ManagedThreadId, e.Message), Logger.MsgType.Warning);
                        return;
                    }
                    Logger.UltraLogger.Instance.AddToLog(String.Format("Worker: Thread {0} has been aborted.", Tr.ManagedThreadId), Logger.MsgType.Warning);
                }

            }
        }

        public WorkerThreadState GetState()
        {
            return Params.State;
        }
        public void Init(WorkerThreadParams Params)
        {
            this.Params = Params;
            Tr.Abort();
            Tr = new System.Threading.Thread(Params.T.Exec);
            ID = Tr.ManagedThreadId;
            this.Params.State = WorkerThreadState.Loaded;
        }
        /// <summary>
        /// Async start
        /// </summary>
        public void Start()
        {
            if(Tr!=null)
            {
                if(Tr.IsAlive)
                {
                    
                }
                try
                {
                    Tr.Start();
                }
                catch (Exception e)
                {
                    Logger.UltraLogger.Instance.AddToLog(String.Format("Worker: Thread couldn't be started. {0}. Exception message: \"{1}\"", 
                                                                                                              ID, e.Message), 
                                                        Logger.MsgType.Error);
                    return;
                }
                Logger.UltraLogger.Instance.AddToLog(String.Format("Worker: Thread {0} has been started.", 
                                                                ID), Logger.MsgType.Action);
            }

        }

        public void Suspend()
        {
            if(Tr!=null)
            {
                if(Tr.IsAlive)
                {
                    Tr.Suspend();
                }
            }
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
        }
        public void Init(WorkerParams Params)
        {
            this.Params = Params;
            Tasker = new Tasker(Params.TaskerParams);
            InitTheads();
        }
        void InitTheads()
        {
            Threads = new List<IWorkerThread>();
            Threads.AddRange(Enumerable.Repeat<IWorkerThread>(new WorkerThread(new WorkerThreadParams()), Params.MaxThreads));
        }
        void AddTasks(int Count)
        {
            foreach (var T in Tasker.GetTasks(Count))
            {
                TaskQueue.Enqueue(T);
            }
        }
        public void Run()
        {
            throw new NotImplementedException();
            AddTasks(Params.MaxThreads - TaskQueue.Count);
            while ((TaskQueue.Count != 0) && (!Tasker.IsFinished()))
            {
                foreach (var Thread in Threads)
                {
                    if (Thread.GetState() == WorkerThreadState.Free)
                    {
                        Task Task;
                        if (TaskQueue.TryDequeue(out Task))
                        {
                            Thread.Init(new WorkerThreadParams(Task));
                            Thread.Start();
                        }
                    }
                }
                AddTasks(Params.MaxThreads - TaskQueue.Count);
            }
            //long SecNow = DateTime.Now.Second;
        }
        public void AsyncRun()
        {
            throw new NotFiniteNumberException();
        }
    }
}
