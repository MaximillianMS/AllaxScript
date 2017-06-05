using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Allax
{
    struct Solver
    {
        public Solver(ISolver s, bool isUsedForBruteForce = false)
        {
            this.S = s;
            this.IsUsedForBruteForce = isUsedForBruteForce;
        }
        public ISolver S;
        public bool IsUsedForBruteForce;
    }
    public delegate void CallbackAddTask(Task T);
    public struct MultiTime
    {
        Int64 UnixTime;
        Int64 Ticks;
    }
    public struct ExtraParams
    {
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
            P = new Prevalence(0,0,Way.layers[1].blocks[0].active_inputs.Count);
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
        public TaskerParams(ISPNet Net, Algorithm Alg)
        {
            this.Net = Net;
            this.Alg = Alg;
        }
        public ISPNet Net;
        public Algorithm Alg;
    }
}
