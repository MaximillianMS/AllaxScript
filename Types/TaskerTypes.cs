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
        public ExtraParams(ISolver Solver)
        {
            this.Solver = Solver;
            Weight = 0;
            Time = new MultiTime();
        }
        public ISolver Solver;
        public int Weight;
        public MultiTime Time;
    }
    public struct Task
    {
        SPNetWay Way;
        long CurrentCorrelation;
        ExtraParams Params;
        public Task(SPNetWay Way, long CurrentCorrelation, ExtraParams Params)
        {
            this.Way = Way;
            this.Params = Params;
            this.CurrentCorrelation = CurrentCorrelation;
        }
        public void Exec()
        {
            Params.Solver.Solve(WayConverter.CloneWay(Way), CurrentCorrelation);
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
