using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Allax
{
    public class Tasker:ITasker
    {
        private object syncRoot = new object();
        TaskerParams Params;
        int _rounds_count;
        ConcurrentQueue<Task> _tasks;
        SPNetWay _tempEmptyWay;
        InputsIterator Iter;
        Dictionary<AvailableSolverTypes, Solver> Solvers;
        private bool IsBruteForceTurnedOn;
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
            var Net = this.Params.Engine.GetSPNetInstance();
            _rounds_count = Params.Engine.GetSPNetInstance().GetLayers().Count / 3;
            _tempEmptyWay = WayConverter.ToWay(Net);
            IsBruteForceTurnedOn = false;
            Iter = new InputsIterator(Net.GetSettings().SBoxCount, Net.GetSettings().SBoxSize);
            _tasks = new ConcurrentQueue<Task>();
            InitSolvers();
            ProcessRules();
        }
        void ProcessRules()
        {
            var Net = Params.Engine.GetSPNetInstance();
            //throw new NotImplementedException();
            for (int i=0;i<Params.Alg.Rules.Count;i++)
            {
                var Rule = Params.Alg.Rules[i];
                if (Rule.UseCustomInput==true)
                {
                    var SolParam = new SolverParams(WayConverter.ToWay(Net, Rule.Input), Params.Engine, Params.Alg.Type, Rule.MaxActiveBlocksOnLayer);
                    var T = new Task(Solvers[Rule.SolverType].S, SolParam, new ExtraParams());
                    _tasks.Enqueue(T);
                }
                else
                {
                    var S = Solvers[Rule.SolverType];
                    S.IsUsedForBruteForce = true;
                    S.MaxActiveBlocksOnLayer = Rule.MaxActiveBlocksOnLayer;
                    Solvers[Rule.SolverType] = S;
                    IsBruteForceTurnedOn = true;
                }
            }
            //Warning! Unoptimized code!!!
            if (IsBruteForceTurnedOn)
            {
                var temp = Iter;
                SolverInputs NextInput;
                foreach (var S in Solvers)
                {
                    if (S.Value.IsUsedForBruteForce)
                    {
                        Iter = new InputsIterator(Net.GetSettings().SBoxCount, Net.GetSettings().SBoxSize);
                        while (!Iter.IsFinished())
                        {
                            NextInput = Iter.NextState();
                            var ws = WayConverter.ToWay(Net, NextInput);

                            _tasks.Enqueue(new Task(S.Value.S, new SolverParams(ws, Params.Engine, Params.Alg.Type, S.Value.MaxActiveBlocksOnLayer)));
                        }
                    }
                }
                Iter = temp;
            }
        }
        void InitSolvers()
        {
            Solvers = new Dictionary<AvailableSolverTypes, Solver> {
                { AvailableSolverTypes.HeuristicSolver, new Solver(new HeuristicSolver()) },
                { AvailableSolverTypes.BaseSolver, new Solver(new BaseSolver()) }
            };
        }
        void AnalysePreviousTasks()
        {
            throw new NotImplementedException();
        }
        public List<Task> GetTasks(int count)
        {
            var ret = new List<Task>();
            for (int i = 0; (i < count) && (_tasks.Count > 0);)
            {
                if (_tasks != null)
                    if (_tasks.Count > 0)
                    {
                        Task T;
                        if (!_tasks.TryDequeue(out T))
                        {
                            Logger.UltraLogger.Instance.AddToLog("Tasker: Cant dequeue predefined tasks", Logger.MsgType.Error);
                        }
                        else
                        {
                            ret.Add(T);
                            i++;
                            continue;
                        }
                    }

            }
            return ret;
        }

        public bool IsFinished()
        {
            //throw new NotImplementedException();
            return Iter.IsFinished()/*&&(_tasks.Count==0)*/;
        }
    }
    class InputsIterator
    {
        int CurrentBlock;
        int BlocksCount;
        int BlockLength;
        List<InputsIteratorBlock> Blocks;
        public InputsIterator(int BlocksCount, int BlockLength)
        {
            this.BlocksCount = BlocksCount;
            this.BlockLength = BlockLength;
            if (this.BlocksCount <= 0 || this.BlockLength <= 0)
            {
                Logger.UltraLogger.Instance.AddToLog("Iterator: Wrong init params.", Logger.MsgType.Error);
                throw new NotImplementedException();
            }
            Blocks = new List<InputsIteratorBlock>(this.BlocksCount);
            Blocks.AddRange(Enumerable.Repeat(0, this.BlocksCount).Select(i=> new InputsIteratorBlock(this.BlockLength)).ToList());
            CurrentBlock = 0;
        }
        public SolverInputs NextState()
        {
            //lock (syncRoot)
            {
                if (Blocks[CurrentBlock].IsFinished())
                {
                    CurrentBlock++;
                    if (CurrentBlock >= BlocksCount)
                    {
                        return new SolverInputs();
                    }
                }
                var CurrentBlockInput = Blocks[CurrentBlock].NextState();
                CurrentBlockInput = CurrentBlockInput << ((BlocksCount - (CurrentBlock + 1)) * BlockLength);
                return new SolverInputs(CurrentBlockInput, BlockLength * BlocksCount);
            }

        }
        public bool IsFinished()
        {
            return Enumerable.Range(0, BlocksCount).All(x => Blocks[x].IsFinished());
        }
    }
    class InputsIteratorBlock
    {
        //private object syncRoot = new object();
        //List<bool> input;
        int weight;
        bool finished;
        int StatesPassed;
        int Length;
        public void ResetState()
        {
            StatesPassed = 0;
            finished = false;
        }
        public InputsIteratorBlock(int BlockLength)
        {
            //input = new List<bool>(BlockLength);
            //input.AddRange(Enumerable.Repeat(false, BlockLength));
            Length = BlockLength;
            StatesPassed = 0;
            finished = false;
        }
        public long NextState()
        {
            //lock (syncRoot)
            {
                if (!IsFinished())
                {
                    //input = WayConverter.ToList(StatesPassed + 1, input.Count);
                    StatesPassed++;
                    return StatesPassed;
                }
                else
                {
                    //Cyclic
                    ResetState();
                    return NextState();
                }
            }
        }
        public bool IsFinished()
        {
            if(StatesPassed>=((1<<Length)-1))
            {
                finished = true;
            }
            else
            {
                finished = false;
            }
            return finished;
        }
    }
}
