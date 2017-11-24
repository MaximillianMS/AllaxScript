using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Linq;
using System.Text;

namespace Allax
{
    class TrueWorker : IWorker
    {
        public event TASKHANDLER ALLTASKSDONE;
        public event TASKHANDLER TASKDONE;
        private static readonly object syncRoot = new object();
        BackgroundWorker BW = new BackgroundWorker();
        WorkerParams Params;
        List<ITask> TaskList = new List<ITask>();
        public TrueWorker(WorkerParams Params)
        {
            Init(Params);
        }
        public void AddTasks(int count)
        {
            TaskList.AddRange(Params.Engine.GetTaskerInstance().DequeueTasks(count));
        }

        public void AsyncRun()
        {
            if (BW != null)
                BW.RunWorkerAsync();
        }

        public void Init(WorkerParams Params)
        {
            BW = new BackgroundWorker();
            this.Params = Params;
            TaskList = new List<ITask>();
            AddTasks(Params.MaxThreads);
            foreach(var T in TaskList)
            {
                Parallel.Invoke(T.Exec);
            }
        }

        private void BW_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            throw new NotImplementedException();

        }

        public void Pause()
        {
            throw new NotImplementedException();
        }

        public void Resume()
        {
            throw new NotImplementedException();
        }

        public void Run()
        {
            //he-he
            AsyncRun();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~TrueWorker() {
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
}
