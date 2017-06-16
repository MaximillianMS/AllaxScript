using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Allax
{
    struct Solver
    {
        public Solver(ISolver s, bool isUsedForBruteForce = false, int MaxActiveBlocksOnLayer = 2)
        {
            this.MaxActiveBlocksOnLayer = MaxActiveBlocksOnLayer;
            this.S = s;
            this.IsUsedForBruteForce = isUsedForBruteForce;
        }
        public ISolver S;
        public bool IsUsedForBruteForce;
        public int MaxActiveBlocksOnLayer;
    }
    public delegate void CallbackAddTask(Task T);
    public struct MultiTime
    {
        Int64 UnixTime;
        Int64 Ticks;
    }
    public struct ExtraParams
    {
        public ExtraParams(int Weight = 0)
        {
            this.Weight = Weight;
            //this.Time = Time;
        }
        public int Weight;
        //public MultiTime Time;
    }
    public struct Task
    {
        public SPNetWay GetWay()
        {
            return SolParams.Way;
        }
        public ISolver Solver;
        //long CurrentCorrelation;
        ExtraParams Params;
        SolverParams SolParams;
        public Task(ISolver Solver, SolverParams SolParams, ExtraParams Params)
        {
            this.Solver = Solver;
            this.SolParams = SolParams;
            this.Params = Params;
            //this.CurrentCorrelation = CurrentCorrelation;
        }
        public Task(ISolver Solver, SolverParams SolParams)
        {
            this.Solver = Solver;
            this.SolParams = SolParams;
            this.Params = new ExtraParams();
        }
        public void Exec()
        {
            Solver.Solve(SolParams);
        }
    }
    public struct TaskerParams
    {
        public TaskerParams(IEngine Engine, Algorithm Alg)
        {
            this.Engine = Engine;
            this.Alg = Alg;
        }
        public IEngine Engine;
        public Algorithm Alg;
    }
}
