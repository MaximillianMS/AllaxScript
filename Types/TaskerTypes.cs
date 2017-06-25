using System;

namespace Allax
{
    public struct Solver
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
    //public delegate void CallbackAddTask(Task T);
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
            this.StartTime = DateTime.Now;
            this.EndTime = DateTime.Now;
        }
        public int Weight;
        public DateTime StartTime;
        public DateTime EndTime;
        //public MultiTime Time;
    }
    public interface ITask
    {
        void Exec();
    }
    public class Task:ITask
    {
        public Task()
        {
            Solver = null;
        }
        public SPNetWay GetWay()
        {
            return SolParams.Way;
        }
        public ISolver Solver;
        public ExtraParams Params;
        SolverParams SolParams;
        public Task(ISolver Solver, SolverParams SolParams, ExtraParams Params)
        {
            this.Solver = Solver;
            this.SolParams = SolParams;
            this.Params = Params;
        }
        public Task(ISolver Solver, SolverParams SolParams)
        {
            this.Solver = Solver;
            this.SolParams = SolParams;
            this.Params = new ExtraParams();
        }
        public virtual void Exec()
        {
            Params.StartTime = DateTime.Now;
            Solver.Solve(SolParams);
            Params.EndTime = DateTime.Now;
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
