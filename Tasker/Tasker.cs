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
        List<Task> GetTasks(int Count);
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
            Iter = new OpenTextInputWeightIterator(Params.Net.GetSettings().sblock_count, Params.Net.GetSettings().word_length / Params.Net.GetSettings().sblock_count);
            _tasks = new ConcurrentQueue<Task>();
            InitSolvers();
            //ProcessRules();
        }
        void ProcessRules()
        {
            throw new NotImplementedException();
            var Alg = Params.LinAlg;
        }
        void InitSolvers()
        {
            Solvers = new Dictionary<AvailableSolverTypes, ISolver> {
                { AvailableSolverTypes.BaseSolver, new BaseSolver(new SolverParams(Params.Net, AddTask)) },
                { AvailableSolverTypes.HeuristicSolver, new HeuristicSolver() }
            };
        }
        void AnalysePreviousTasks()
        {
            throw new NotImplementedException();
        }
        public List<Task> GetTasks(int count)
        {
            lock (syncRoot)
            {
                var ret = new List<Task>();
                for (int i = 0; (i < count) && Iter.IsFinished(); i++)
                {
                    if(_tasks!=null)
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
                            continue;
                        }
                    }
                    OpenTextInput NextInput = Iter.NextState();
                    SPNetWay ws = WayConverter.ToWay(Params.Net, NextInput);
                    ret.Add(new Task(ws, Solvers[AvailableSolverTypes.BaseSolver]));
                }
                return ret;
            }
        }

        public bool IsFinished()
        {
            //throw new NotImplementedException();
            return Iter.IsFinished()/*&&(_tasks.Count==0)*/;
        }
    }
    class OpenTextInputWeightIterator
    {
        int CurrentBlock;
        int BlocksCount;
        int BlockLength;
        List<OpenTextInputTextBlock> Blocks;
        public OpenTextInputWeightIterator(int BlocksCount, int BlockLength)
        {
            this.BlocksCount = BlocksCount;
            this.BlockLength = BlockLength;
            if (this.BlocksCount <= 0 || this.BlockLength <= 0)
            {
                Logger.UltraLogger.Instance.AddToLog("OTIWIterator: Wrong init params.", Logger.MsgType.Error);
                throw new NotImplementedException();
            }
            Blocks = new List<OpenTextInputTextBlock>(this.BlocksCount);
            Blocks.AddRange(Enumerable.Repeat(new OpenTextInputTextBlock(this.BlockLength), this.BlocksCount));
            CurrentBlock = 0;
        }
        public OpenTextInput NextState()
        {
            //lock (syncRoot)
            {
                if (Blocks[CurrentBlock].IsFinished())
                {
                    CurrentBlock++;
                    if (CurrentBlock >= BlocksCount)
                    {
                        return new OpenTextInput();
                    }
                }
                var CurrentBlockInput = Blocks[CurrentBlock].NextState();
                CurrentBlockInput = CurrentBlockInput << ((BlocksCount - (CurrentBlock + 1)) * BlockLength);
                return new OpenTextInput(CurrentBlockInput, BlockLength * BlocksCount);
            }

        }
        public bool IsFinished()
        {
            throw new NotImplementedException();
        }
    }
    class OpenTextInputTextBlock
    {
        private object syncRoot = new object();
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
        public OpenTextInputTextBlock(int BlockLength)
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
            if(StatesPassed>=(1<<Length))
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
    public struct OpenTextInput
    {
        public OpenTextInput(List<bool> Input)
        {
            input = Input;
            weight = 0;
        }
        public OpenTextInput(long Input, int length)
        {
            input = WayConverter.ToList(Input, length);
            weight = 0;
        }
        public List<bool> input;
        public int weight;
    }
}
