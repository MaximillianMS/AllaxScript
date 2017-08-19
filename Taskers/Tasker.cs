using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Allax
{
    public class Tasker : ITasker
    {
        private object syncRoot = new object();
        TaskerParams Params;
        int _rounds_count;
        Queue<ITask> _tasks;
        SPNetWay _tempEmptyWay;
        //InputsIterator Iter;
        Dictionary<AvailableSolverTypes, Solver> Solvers;
        Dictionary<AvailableSolverTypes, InputsIterator> Iterators;
        private bool IsBruteForceTurnedOn;
        public Tasker(TaskerParams Params)
        {
            Init(Params);
            InitSolvers();
            Reset();
            ProcessRules(Params.Alg.Rules);
            //AddAllBruteForceTasks();
        }
        public void Reset()
        {
            IsBruteForceTurnedOn = false;
            _tasks = new Queue<ITask>();
            foreach (var It in Iterators)
                It.Value.Reset();
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
            Solvers = new Dictionary<AvailableSolverTypes, Solver>();
            Iterators = new Dictionary<AvailableSolverTypes, InputsIterator>();
        }
        public void AddAllBruteForceTasks()
        {
            var Net = Params.Engine.GetSPNetInstance();
            //Warning! Unoptimized code!!!
            if (IsBruteForceTurnedOn)
            {
                SolverInputs NextInput;
                foreach (var T in AvailableSolverTypes.GetAllTypes())
                {
                    if (Solvers.ContainsKey(T))
                    {
                        var S = Solvers[T];
                        if (S.IsUsedForBruteForce)
                        {
                            var Iter = Iterators[T];
                            while (!Iter.IsFinished())
                            {
                                NextInput = Iter.NextState();
                                var ws = WayConverter.ToWay(Net, NextInput);

                                _tasks.Enqueue(new Task(S.S, new SolverParams(ws, Params.Engine, Params.Alg.Type, S.MaxActiveBlocksOnLayer, S.CheckPrevalence)));
                            }
                        }
                    }
                }
            }
        }
        public void ProcessRules(List<Rule> Rules)
        {
            var Net = Params.Engine.GetSPNetInstance();
            //throw new NotImplementedException();
            for (int i = 0; i < Rules.Count; i++)
            {
                var Rule = Rules[i];
                if (Rule.UseCustomInput == true)
                {
                    var SolParam = new SolverParams(WayConverter.ToWay(Net, Rule.Input), Params.Engine, Params.Alg.Type, Rule.MaxActiveBlocksOnLayer, Rule.CheckPrevalence);
                    var T = new Task(Solvers[Rule.SolverType].S, SolParam, new ExtraParams());
                    _tasks.Enqueue(T);
                }
                else
                {
                    var S = Solvers[Rule.SolverType];
                    S.IsUsedForBruteForce = true;
                    S.MaxActiveBlocksOnLayer = Rule.MaxActiveBlocksOnLayer;
                    S.CheckPrevalence = Rule.CheckPrevalence;
                    if (!Iterators.ContainsKey(Rule.SolverType))
                    {
                        Iterators.Add(Rule.SolverType, new InputsIterator(Net.GetSettings().SBoxCount, Net.GetSettings().SBoxSize, Rule.MaxStartBlocks));
                    }
                    else
                    {
                        Iterators[Rule.SolverType] = new InputsIterator(Net.GetSettings().SBoxCount, Net.GetSettings().SBoxSize, Rule.MaxStartBlocks);
                    }
                    Solvers[Rule.SolverType] = S;
                    IsBruteForceTurnedOn = true;
                }
            }
        }
        public void InitSolvers(Dictionary<AvailableSolverTypes, Solver> Solvers = null)
        {
            if (Params.Engine != null)
                this.Solvers = Params.Engine.GetSolvers().ToDictionary(i=>i.Key, i=>new Solver(i.Value));
            else
                throw new NotImplementedException();
        }
        void AnalysePreviousTasks()
        {
            throw new NotImplementedException();
        }
        public List<ITask> DequeueTasks(int count)
        {
            var ret = new List<ITask>();
            //             if (count < 0)
            //                 count = _tasks.Count;
            while ((ret.Count < count) && (_tasks.Count > 0))
            {
                //                 if (!_tasks.TryDequeue(out T))
                //                 {
                //                     Logger.UltraLogger.Instance.AddToLog("Tasker: Cant dequeue predefined tasks", Logger.MsgType.Error);
                //                     throw new NotImplementedException();
                //                 }
                //                 else
                 var  T = _tasks.Dequeue();
                ret.Add(T);
                continue;
            }
            if(ret.Count<count)
            {
                foreach(var S in Solvers)
                {
                    if(S.Value.IsUsedForBruteForce)
                    {
                        var Iter = Iterators[S.Key];
                        while (!Iter.IsFinished() && ret.Count < count)
                        {
                            var NextInput = Iter.NextState();
                            var ws = WayConverter.ToWay(Params.Engine.GetSPNetInstance(), NextInput);
                            ret.Add(new Task(S.Value.S, new SolverParams(ws, Params.Engine, Params.Alg.Type, S.Value.MaxActiveBlocksOnLayer, S.Value.CheckPrevalence)));
                        }
                    }
                    if (ret.Count >= count)
                        break;
                }
            }
            return ret;
        }
        public bool IsFinished()
        {
            //throw new NotImplementedException();
            return AvailableSolverTypes.GetAllTypes().Where(i=>Solvers[i].IsUsedForBruteForce).All(i=>Iterators[i].IsFinished()) && (_tasks.Count==0);
        }
        public ulong GetTasksCount()
        {
            return (ulong)(_tasks.Count) + AvailableSolverTypes.GetAllTypes().Where(i => Solvers[i].IsUsedForBruteForce).Select(i=>Iterators[i]).Aggregate<InputsIterator, ulong>(0, (acc, x) => acc + x.GetStatesCount());
        }
    }
    class InputsIterator
    {
        int CurrentBlock;
        int CurrentMask;
        int MaxMask;
        int BlocksCount;
        int BlockLength;
        int MaxBlocksOnInput;
        ulong Factorial(ulong i)
        {
            if (i <= 1)
                return 1;
            return i * Factorial(i - 1);
        }
        public ulong GetStatesCount()
        {
            ulong ret=0;
            for(int i=1;i<MaxBlocksOnInput+1;i++)
            {
                ulong statesCount = 1;
                for(int j=0;j<i;j++)
                {
                    statesCount *= (((ulong)(1 << BlockLength))-1);
                }
                ret += statesCount * (Factorial((ulong)BlocksCount) / (Factorial((ulong)i) * Factorial((ulong)(BlocksCount - i))));
            }
            return ret;
        }
        List<InputsIteratorBlock> Blocks = new List<InputsIteratorBlock>();
        public void Reset()
        {
            foreach(var B in Blocks)
            {
                B.ResetState();
            }
        }
        public InputsIterator(int BlocksCount, int BlockLength, int MaxBlocksOnInput = 1)
        {
            this.BlocksCount = BlocksCount;
            this.BlockLength = BlockLength;
            this.MaxBlocksOnInput = (MaxBlocksOnInput < BlocksCount) ? MaxBlocksOnInput : BlocksCount;
            if (this.BlocksCount <= 0 || this.BlockLength <= 0)
            {
                Logger.UltraLogger.Instance.AddToLog("Iterator: Wrong init params.", Logger.MsgType.Error);
                throw new NotImplementedException();
            }
            Blocks = new List<InputsIteratorBlock>(this.BlocksCount);
            Blocks.AddRange(Enumerable.Repeat(0, this.BlocksCount).Select(i => new InputsIteratorBlock(this.BlockLength)).ToList());
            CurrentBlock = 0;
            CurrentMask = 1;
            MaxMask = (1 << (BlocksCount)) - 1;
        }
        bool IsBlockInMask(int Block, int Mask)
        {
            return (1 << Block/*(BlocksCount - 1 - Block)*/ & CurrentMask) != 0;
        }
        public ulong Concat(ulong CurrentInput, byte Value, int CurrentBlock)
        {
            CurrentInput |= (ulong)Value << (BlockLength * (BlocksCount - 1 - CurrentBlock));
            return CurrentInput;
        }
        public bool CheckMaxCondition(int Mask)
        {
            int counter = 0;
            for (int i = 0; i < BlocksCount; i++)
            {
                counter += ((((1 << i) & Mask) > 0) ? 1 : 0);
            }
            return counter <= MaxBlocksOnInput;
        }
        public SolverInputs NextState()
        {
            CurrentBlock = 0;
            ulong CurrentBlockInput = 0;
            for (; CurrentBlock < BlocksCount; CurrentBlock++)
            {
                if (IsBlockInMask(CurrentBlock, CurrentMask))
                {
                    if (Blocks[CurrentBlock].IsFinished())
                    {
                        CurrentBlockInput = Concat(CurrentBlockInput, (byte)Blocks[CurrentBlock].NextState(), CurrentBlock);
                        continue;
                    }
                    CurrentBlockInput = Concat(CurrentBlockInput, (byte)Blocks[CurrentBlock].NextState(), CurrentBlock);
                    CurrentBlock++;
                    for (; CurrentBlock < BlocksCount; CurrentBlock++)
                    {
                        if (IsBlockInMask(CurrentBlock, CurrentMask))
                        {
                            var Value = (byte)Blocks[CurrentBlock].CurrentState();
                            if (Value == 0)
                                Value = (byte)Blocks[CurrentBlock].NextState();
                            CurrentBlockInput = Concat(CurrentBlockInput, Value, CurrentBlock);
                        }
                    }
                    return new SolverInputs((long)CurrentBlockInput, BlockLength * BlocksCount);
                }
            }
            CurrentMask++;
            while (!CheckMaxCondition(CurrentMask))
                CurrentMask++;
            foreach (var B in Blocks)
            {
                B.ResetState();
            }
            return NextState();
        }
        public bool IsFinished()
        {
            var NextMask = CurrentMask;
            if (Enumerable.Range(0, BlocksCount).Where(i => IsBlockInMask(i, CurrentMask)).All(x => Blocks[x].IsFinished()))
            {
                NextMask++;
                while (!CheckMaxCondition(NextMask))
                    NextMask++;
            }
            if (NextMask > MaxMask)
            {
                return true;
            }
            else
            {
                return false;
            }
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
        public long CurrentState()
        {
            return StatesPassed;
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
            if (StatesPassed >= ((1 << Length) - 1))
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
