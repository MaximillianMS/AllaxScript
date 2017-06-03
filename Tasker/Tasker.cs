using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Allax
{
    public interface ITasker
    {
        ConcurrentQueue<Task> GetTasks(int Count);
        void AddTask(Task T);
        void Init(TaskerParams Params);
        bool IsFinished();
    }
    public class Tasker:ITasker
    {
        private object syncRoot = new object();
        TaskerParams Params;
        int _rounds_count;
        ConcurrentQueue<Task> _tasks;
        SPNetWay _tempEmptyWay;
        OpenTextInputWeightIterator Iter;
        ISolver Solver;
        Dictionary<AvailableSolverTypes, ISolver> Solvers;
        public Tasker(TaskerParams Params)
        {
            Init(Params);
        }
        public void AddTask(Task T)
        {
            _tasks.Enqueue(T);
        }
        public void Init(TaskerParams Params)
        {
            this.Params = Params;
            _rounds_count = Params.Net.GetLayers().Count / 3;
            _tempEmptyWay = WayConverter.ToWay(Params.Net);
            InitSolvers();
        }
        void InitSolvers()
        {
            Solvers = new Dictionary<AvailableSolverTypes, ISolver> {
                { AvailableSolverTypes.BaseSolver, new BaseSolver(new SolverParams(Params.Net, AddTask)) }
                /*"Heuristics" : new HeuristicSolver()*/
            };
        }
        void AnalysePreviousTasks()
        {
            throw new NotImplementedException();
        }
        public ConcurrentQueue<Task> GetTasks(int count)
        {
            /*if (_tasks.Count != 0)
            {
                AnalysePreviousTasks();
            }*/
            //REWRITE!
            throw new NotImplementedException();
            var ret = new ConcurrentQueue<Task>();
            for (int i = 0; (i < count) && Iter.IsFinished(); i++)
            {
                OpenTextInput NextInput = Iter.NextState();
                SPNetWay ws = WayConverter.ToWay(Params.Net, NextInput);
                ret.Enqueue(new Task(ws, 1, new ExtraParams(Solver)));
            }
            return ret;
        }

        public bool IsFinished()
        {
            //throw new NotImplementedException();
            return Iter.IsFinished()/*&&(_tasks.Count==0)*/;
        }
    }
    class OpenTextInputWeightIterator
    {
        List<OpenTextInputTextBlock> Blocks;
        public OpenTextInput NextState()
        {
            throw new NotImplementedException();
        }
        public bool IsFinished()
        {
            throw new NotImplementedException();
        }
    }
    class OpenTextInputTextBlock
    {
        List<bool> input;
        int weight;
        bool finished;
        int StatesPassed;
        public List<byte> NextState()
        {
            throw new NotImplementedException();
        }
        public bool IsFinished()
        {
            throw new NotImplementedException();
        }
    }
    public struct OpenTextInput
    {
        public List<bool> input;
        public int weight;
    }
}
