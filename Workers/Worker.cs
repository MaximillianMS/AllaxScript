using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Allax
{
    public class WorkerThread : IWorkerThread
    {
        public event JOBSDONEHANDLER JOBSDONE;
        WorkerThreadParams Params;
        public int ID;
        System.Threading.Thread Tr;
        IWorker W;
        public WorkerThread(WorkerThreadParams Params, int ExternalId, IWorker W) : this(ExternalId, W)
        {
            Init(Params);
        }
        public WorkerThread(int ExternalId, IWorker W) : this(ExternalId)
        {
            this.W = W;
        }

        public WorkerThread(int ExternalId)
        {
            if (Tr != null)
                Tr.Abort();
            Tr = new System.Threading.Thread(ThreadWork) { IsBackground = true };
            ID = ExternalId;
        }

        public void Abort()
        {
            if (Tr != null)
            {
                if (Tr.IsAlive)
                {
                    try
                    {
                        Tr.Abort();
                    }
                    catch (Exception e)
                    {
                        Logger.UltraLogger.Instance.AddToLog(String.Format("Worker: Thread {0}. Exception message: \"{1}\"", Tr.ManagedThreadId, e.Message), Logger.MsgType.Error);
                    }
                    finally
                    {
                        Logger.UltraLogger.Instance.AddToLog(String.Format("Worker: Thread {0} has been aborted.", Tr.ManagedThreadId), Logger.MsgType.Warning);
                        Params.State = WorkerThreadState.Stopped;
                    }
                }
            }
        }

        public WorkerThreadState GetState()
        {
            return Params.State;
        }
        private void ThreadWork()
        {
            Params.T.Exec();
            Params.State = WorkerThreadState.Free;
            JOBSDONE.BeginInvoke(this, null, null);
        }
        public void Init(WorkerThreadParams Params)
        {
            this.Params = Params;
            this.Params.State = WorkerThreadState.Loaded;
        }
        /// <summary>
        /// Async start
        /// </summary>
        public void Start()
        {
            if (Tr != null)
            {
                if (Params.State == WorkerThreadState.Started)
                {
                    Logger.UltraLogger.Instance.AddToLog(String.Format("Worker: Thread {0} has already been started.",
                                                                    ID), Logger.MsgType.Error);
                    throw new NotImplementedException();
                    return;
                }
                if (Params.State == WorkerThreadState.Free)
                {
                    Logger.UltraLogger.Instance.AddToLog(String.Format("Worker: Thread {0} is not initialized.",
                                                                    ID), Logger.MsgType.Error);
                    throw new NotImplementedException();
                    return;
                }
                try
                {
                    Tr = new System.Threading.Thread(ThreadWork) { IsBackground = true };
                    Params.State = WorkerThreadState.Started;
                    Tr.Start();
                }
                catch (Exception e)
                {
                    Params.State = WorkerThreadState.Stopped;
                    Logger.UltraLogger.Instance.AddToLog(String.Format("Worker: Thread couldn't be started. {0}. Exception message: \"{1}\"",
                                                                                                              ID, e.Message),
                                                        Logger.MsgType.Error);
                    return;
                }
                finally
                {
                    Logger.UltraLogger.Instance.AddToLog(String.Format("Worker: Thread {0} has been started.",
                                                                    ID), Logger.MsgType.Action);
                }
            }
        }

        public void Suspend()
        {
            if (Tr != null)
            {
                if (Tr.IsAlive && (Params.State == WorkerThreadState.Started))
                {
                    throw new NotImplementedException();
                    Params.State = WorkerThreadState.Paused;
                }
            }
        }

        public void Resume()
        {
            if (Tr != null)
            {
                if (Tr.IsAlive && (Params.State == WorkerThreadState.Paused))
                {
                    throw new NotImplementedException();
                    Params.State = WorkerThreadState.Started;
                }
            }
        }

        public Task GetCurrentTask()
        {
            return this.Params.T;
        }
        ~WorkerThread()
        {
            Dispose();
        }
        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.Abort();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~WorkerThread() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
    public class Worker : IWorker
    {
        public event TASKHANDLER TASKDONE;

        void InitAndRunFreeThreads(IWorkerThread Thread)
        {
            ;
            if(Thread.GetState()==WorkerThreadState.Free)
            {
                TASKDONE.BeginInvoke(Thread.GetCurrentTask(), null, null);
                InitThread(Thread);
            }
            else
            {
                Logger.UltraLogger.Instance.AddToLog("Worker: Thread is not free. Cant init thread after end of previous task.", Logger.MsgType.Error);
                throw new NotImplementedException();
            }
        }
        WorkerParams Params;
        ConcurrentQueue<Task> TaskQueue;
        List<IWorkerThread> Threads;
        public Worker(WorkerParams Params)
        {
            Init(Params);
            //JOBSDONE += new JOBSDONEHANDLER(InitAndRunFreeThreads);
        }
        public void Init(WorkerParams Params)
        {
            this.Params = Params;
            TaskQueue = new ConcurrentQueue<Task>();
            CreateTheads();
        }
        void CreateTheads()
        {
            Threads = new List<IWorkerThread>();
            for(int i=0; i<Params.MaxThreads;i++)
            {
                Threads.Add(new WorkerThread(i, this));
                Threads[i].JOBSDONE += InitAndRunFreeThreads;
            }
        }
        void AddTasks(int Count=1)
        {
            foreach (var T in Params.Engine.GetTaskerInstance().GetTasks(Count))
            {
                TaskQueue.Enqueue(T);
            }
        }
        public bool InitThread(IWorkerThread Thread)
        {
            bool OK = true;
            Task Task;
            if (TaskQueue.Count == 0)
            {
                AddTasks(Params.MaxThreads);
            }
            if (TaskQueue.TryDequeue(out Task))
            {
                Thread.Init(new WorkerThreadParams(Task));
                Thread.Start();
            }
            else
            {
                if (TaskQueue.Count > 0)
                {
                    Logger.UltraLogger.Instance.AddToLog("Worker: Coudn't dequeue Task.", Logger.MsgType.Error);
                    OK = false;

                }
            }
            return OK;
        }
        public void Run()
        {
            AddTasks(Params.MaxThreads - TaskQueue.Count);
            if ((TaskQueue.Count != 0) && (!Params.Engine.GetTaskerInstance().IsFinished()))
            {
                bool OK = true;
                foreach (var Thread in Threads)
                {
                    OK = InitThread(Thread);
                }
                if(!OK)
                {
                    throw new NotImplementedException();
                    throw new Exception();
                }
            }
            //long SecNow = DateTime.Now.Second;
        }

        public void AsyncRun()
        {
            /*throw new NotFiniteNumberException();*/
            //LOL. you really thought that i'd make another function special for you, Yuri? AHAHAHAHAHAHA (C) =^_^= 
            Run();
        }

        public void Pause()
        {
            foreach(var Thread in Threads)
            {
                Thread.Suspend();
            }
        }

        public void Resume()
        {
            foreach (var Thread in Threads)
            {
                Thread.Resume();
            }
        }
        ~Worker()
        {
            Dispose();
        }
        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (Threads != null)
                        foreach (var T in Threads)
                        {
                            T.Dispose();
                        }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
