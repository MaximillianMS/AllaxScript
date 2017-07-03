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
            if (Tr != null)
                Tr.Abort();
            if (this.GetState() == WorkerThreadState.Finished)
                return;
            Tr = new System.Threading.Thread(ThreadWork) { IsBackground = true };
            this.SetState(WorkerThreadState.Loaded);
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
                if (Params.State == WorkerThreadState.Finished)
                    return;
                try
                {
                    //Tr = new System.Threading.Thread(ThreadWork) { IsBackground = true };
                    this.SetState(WorkerThreadState.Started);
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

        public ITask GetCurrentTask()
        {
            return this.Params.T;
        }
        ~WorkerThread()
        {
            Dispose();
        }
        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls
        private static readonly object syncRoot = new object();
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.Abort();
                    this.SetState(WorkerThreadState.Finished);
                    Tr = null;
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

        public void SetState(WorkerThreadState State)
        {
            lock (syncRoot)
            {
                Params.State = State;
            }
        }
        #endregion
    }
    public class Worker : IWorker
    {
        WorkerThreadState State = WorkerThreadState.Free;
        private static readonly object syncRoot = new object();
        public event TASKHANDLER TASKDONE;
        public event TASKHANDLER ALLTASKSDONE;
        void ReInitThread(IWorkerThread Thread)
        {
            if(Thread.GetState()==WorkerThreadState.Free)
            {
                //if(Params.)
                if (!Params.ASync)
                    TASKDONE(Thread.GetCurrentTask());
                else
                    TASKDONE.BeginInvoke(Thread.GetCurrentTask(), null, null);
                InitThread(Thread);
            }
            else
            {
                if (Thread.GetState() != WorkerThreadState.Finished)
                {
                    Logger.UltraLogger.Instance.AddToLog("Worker: Thread is not free. Cant init thread after end of previous task.", Logger.MsgType.Error);
                    //throw new NotImplementedException();
                }
            }
        }
        WorkerParams Params;
        Queue<ITask> TaskQueue;
        List<IWorkerThread> Threads;
        public Worker(WorkerParams Params)
        {
            Init(Params);
            //JOBSDONE += new JOBSDONEHANDLER(InitAndRunFreeThreads);
        }
        public void Init(WorkerParams Params)
        {
            this.Params = Params;
            TaskQueue = new Queue<ITask>();
            //AddTasks(-1);
            CreateTheads();
            State = WorkerThreadState.Loaded;
        }
        void CreateTheads()
        {
            Threads = new List<IWorkerThread>();
            for(int i=0; i<Params.MaxThreads;i++)
            {
                Threads.Add(new WorkerThread(i, this));
                Threads[i].JOBSDONE += ReInitThread;
            }
        }
        public virtual void AddTasks(int Count=1)
        {
            foreach (var T in Params.Engine.GetTaskerInstance().DequeueTasks(Count))
            {
                TaskQueue.Enqueue(T);
            }
        }
        bool InitThread(IWorkerThread Thread)
        {
            bool OK = true;
            ITask Task;
            lock (syncRoot)
            {
                try
                {
                    if (TaskQueue.Count == 0)
                    {
                        AddTasks(Params.MaxThreads * 1000);
                    }
                    if (TaskQueue.Count > 0)
                    {

                        if (Thread.GetState() == WorkerThreadState.Free && (this.State != WorkerThreadState.Finished))
                        {
                            Task = TaskQueue.Dequeue();
                            Thread.Init(new WorkerThreadParams(Task));
                        }
                        else
                        {
                            if (Thread.GetState() == WorkerThreadState.Finished)
                            {
                                ;
                            }
                            else
                            {
                                return false;// throw new Exception();
                            }
                        }
                        if (Thread.GetState() == WorkerThreadState.Loaded)
                            Thread.Start();
                    }
                    else
                    {
                        //                     if (TaskQueue.Count > 0)
                        //                     {
                        //                         Logger.UltraLogger.Instance.AddToLog("Worker: Coudn't dequeue Task.", Logger.MsgType.Error);
                        //                         OK = false;
                        //                         throw new NotImplementedException();
                        // 
                        //                     }
                        //                     else
                        //                     {
                        if (State != WorkerThreadState.Stopped)
                        {
                            Thread.SetState(WorkerThreadState.Finished);
                            if (Threads.All(x => x.GetState() == WorkerThreadState.Finished))
                            {
                                State = WorkerThreadState.Stopped;
                                if (!Params.ASync)
                                    ALLTASKSDONE(new Task());
                                else
                                    ALLTASKSDONE.BeginInvoke(new Task(), null, null);
                            }
                        }
                    }
                }
                catch { }
            }
            return OK;
        }
        public virtual void Run()
        {
            State = WorkerThreadState.Started;
            AddTasks(Params.MaxThreads - TaskQueue.Count);
            if ((TaskQueue.Count != 0) /*&& (!Params.Engine.GetTaskerInstance().IsFinished())*/)
            {
                bool OK = true;
                foreach (var Thread in Threads)
                {
                    OK = InitThread(Thread);
                }
                if(!OK)
                {
                    //throw new NotImplementedException();
                    //throw new Exception();
                }
            }
            else
            {
                if (!Params.ASync)
                    ALLTASKSDONE(new Task());
                else
                    ALLTASKSDONE.BeginInvoke(new Task(), null, null);
            }
        }
        public virtual void AsyncRun()
        {
            /*throw new NotFiniteNumberException();*/
            //LOL. you really thought that i'd make another function special for you, Yuri? AHAHAHAHAHAHA (C) =^_^= 
            Run();
        }

        public virtual void Pause()
        {
            foreach(var Thread in Threads)
            {
                Thread.Suspend();
            }
        }

        public virtual void Resume()
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
                    Threads = null;
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
