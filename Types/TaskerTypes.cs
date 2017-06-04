using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Allax
{

    public delegate void CallbackAddTask(Task T);
    public struct MultiTime
    {
        Int64 UnixTime;
        Int64 Ticks;
    }
    public struct ExtraParams
    {
        public ExtraParams()
        {
            Weight = 0;
            Time = new MultiTime();
        }
        public int Weight;
        public MultiTime Time;
    }
    public struct Task
    {
        SPNetWay Way;
        public ISolver Solver;
        //long CurrentCorrelation;
        ExtraParams Params;
        Prevalence P;
        public Task(SPNetWay Way, ISolver Solver, Prevalence P, ExtraParams Params)
        {
            this.Way = Way;
            this.Solver = Solver;
            this.P = P;
            this.Params = Params;
            //this.CurrentCorrelation = CurrentCorrelation;
        }
        public Task(SPNetWay Way, ISolver Solver)
        {
            this.Way = Way;
            this.Solver = Solver;
            P = new Prevalence();
            Params = new ExtraParams();
            //this.CurrentCorrelation = CurrentCorrelation;
        }
        public void Exec()
        {
            Solver.Solve(WayConverter.CloneWay(Way), P);
        }
    }
    public struct TaskerParams
    {
        public TaskerParams(ISPNet Net, LinearAlg LinAlg, DifferAlg DifAlg)
        {
            this.Net = Net;
            this.DifAlg = DifAlg;
            this.LinAlg = LinAlg;
        }
        public ISPNet Net;
        public LinearAlg LinAlg;
        public DifferAlg DifAlg;
    }
}
