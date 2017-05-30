using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Allax
{
    public struct WorkerParams
    {
        public WorkerParams(ISPNet Net, int MaxThreads)
        {
            this.Net = Net;
            this.MaxThreads = MaxThreads;
        }
        public ISPNet Net;
        public int MaxThreads;
    }
    public interface IWorker
    {
        void Init(WorkerParams Params);
        void Run();
    }
    public class Worker:IWorker
    {
        WorkerParams Params;
        ITaskConstructor Tasker;
        public Worker(WorkerParams Params)
        {
            Init(Params);
            Tasker = new TaskConstructor(new TaskConstructorParams(Params.Net));
        }
        public void Init(WorkerParams Params)
        {
            this.Params = Params;
        }
        public void Run()
        {
            throw new NotImplementedException();
        }
    }
}
