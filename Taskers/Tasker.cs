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
        ConcurrentQueue<Task> _tasks;
        SPNetWay _tempEmptyWay;
        InputsIterator Iter;
        Dictionary<AvailableSolverTypes, Solver> Solvers;
        private bool IsBruteForceTurnedOn;
        public Tasker(TaskerParams Params)
        {
            Init(Params);
            InitSolvers();
            ProcessRules(Params.Alg.Rules);
            AddBruteForceTasks();
        }
        public void Reset()
        {
            IsBruteForceTurnedOn = false;
            _tasks = new ConcurrentQueue<Task>();
            if (Params.Engine != null)
            {
                var Net = Params.Engine.GetSPNetInstance();
                if (Net != null)
                    Iter = new InputsIterator(Net.GetSettings().SBoxCount, Net.GetSettings().SBoxSize);
            }
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

            Reset();
        }
        public void AddBruteForceTasks()
        {
            var Net = Params.Engine.GetSPNetInstance();
            //Warning! Unoptimized code!!!
            if (IsBruteForceTurnedOn)
            {
                var temp = Iter;
                SolverInputs NextInput;
                foreach (var S in Solvers)
                {
                    if (S.Value.IsUsedForBruteForce)
                    {
                        Iter = new InputsIterator(Net.GetSettings().SBoxCount, Net.GetSettings().SBoxSize, S.Value.MaxStartBlocks);
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
        public void ProcessRules(List<Rule> Rules)
        {
            var Net = Params.Engine.GetSPNetInstance();
            //throw new NotImplementedException();
            for (int i = 0; i < Rules.Count; i++)
            {
                var Rule = Rules[i];
                if (Rule.UseCustomInput == true)
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
                    S.MaxStartBlocks = Rule.MaxStartBlocks;
                    Solvers[Rule.SolverType] = S;
                    IsBruteForceTurnedOn = true;
                }
            }
        }
        public void InitSolvers(Dictionary<AvailableSolverTypes, Solver> Solvers = null)
        {
            if (Solvers == null && Params.Engine != null)
                this.Solvers = Params.Engine.GetSolvers();
            else
                throw new NotImplementedException();
        }
        void AnalysePreviousTasks()
        {
            throw new NotImplementedException();
        }
        public List<Task> GetTasks(int count)
        {
            var ret = new List<Task>();
            if (count < 0)
                count = _tasks.Count;
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
        int CurrentMask;
        int MaxMask;
        int BlocksCount;
        int BlockLength;
        int MaxBlocksOnInput;
        List<InputsIteratorBlock> Blocks;
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
