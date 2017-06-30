using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Allax;
namespace CellAllax
{
    class EngineForTaskerAndWorker : IEngine
    {
        public event TASKHANDLER ONALLTASKSDONE;
        public event PROGRESSHANDLER ONPROGRESSCHANGED;
        public event ADDSOLUTIONHANDLER ONSOLUTIONFOUND;
        public event TASKHANDLER ONTASKDONE;
        public ITasker TheTasker;
        public IWorker TheWorker;
        public void AbortAnalisys()
        {
            throw new NotImplementedException();
        }

        public ISPNet CreateSPNetInstance(SPNetSettings Settings)
        {
            throw new NotImplementedException();
        }

        public Prevalence GetMultiThreadPrevalence()
        {
            throw new NotImplementedException();
        }

        public ISBlockDB GetSBlockDBInstance()
        {
            throw new NotImplementedException();
        }

        public EngineSettings GetSettings()
        {
            throw new NotImplementedException();
        }

        public Dictionary<AvailableSolverTypes, ISolver> GetSolvers()
        {
            throw new NotImplementedException();
        }

        public ISPNet GetSPNetInstance()
        {
            throw new NotImplementedException();
        }

        public ITasker GetTaskerInstance()
        {
            return TheTasker;
        }

        public IWorker GetWorkerInstance()
        {
            return TheWorker;
        }

        public void Init(EngineSettings Settings)
        {
            throw new NotImplementedException();
        }

        public ISBlockDB InjectSBlockDB(FileStream FS, bool xml = false)
        {
            throw new NotImplementedException();
        }

        public void PerformAnalisys(AnalisysParams Params)
        {
            throw new NotImplementedException();
        }

        public void SerializeDB(FileStream FS, bool xml = false)
        {
            throw new NotImplementedException();
        }

        public void SetMultiThreadPrevalence(Prevalence P)
        {
            throw new NotImplementedException();
        }
    }
}
