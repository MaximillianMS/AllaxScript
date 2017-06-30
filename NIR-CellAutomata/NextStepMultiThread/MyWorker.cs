using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Allax.Cell
{
    class SignalWorker : Allax.Worker
    {
        public ManualResetEvent oSignalEvent = new ManualResetEvent(false);
        public SignalWorker(Allax.WorkerParams Params) : base(Params)
        {
            ALLTASKSDONE += SignalWorker_ALLTASKSDONE;
            TASKDONE += SignalWorker_TASKDONE;
        }

        private void SignalWorker_TASKDONE(Allax.ITask T)
        {
            ;
        }

        private void SignalWorker_ALLTASKSDONE(Allax.ITask T)
        {
            oSignalEvent.Set();
        }
    }
}
