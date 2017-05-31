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
    public struct WorkerThreadParams
    {
        public Task T;
    }
}
