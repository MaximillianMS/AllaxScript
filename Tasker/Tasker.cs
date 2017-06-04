﻿using System;
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
        InputsIterator Iter;
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
            Iter = new InputsIterator(Params.Net.GetSettings().sblock_count, Params.Net.GetSettings().word_length / Params.Net.GetSettings().sblock_count);
            _tasks = new ConcurrentQueue<Task>();
            InitSolvers();
            ProcessRules();
        }
        void ProcessRules()
        {
            throw new NotImplementedException();
            for(int i=0;i<Params.Alg.Rules.Count;i++)
            {
                var Rule = Params.Alg.Rules[i];
                if (Rule.UseCustomInput==true)
                {
                    var T = new Task(WayConverter.ToWay(Params.Net, Rule.Input), Solvers[Rule.SolverType]);
                    _tasks.Enqueue(new Task());
                }
            }
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
            var ret = new List<Task>();
            for (int i = 0; (i < count) && (!Iter.IsFinished() || _tasks.Count > 0); i++)
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
                            continue;
                        }
                    }
                SolverInputs NextInput;
                lock (syncRoot)
                {
                    NextInput = Iter.NextState();
                }
                SPNetWay ws = WayConverter.ToWay(Params.Net, NextInput);
                ret.Add(new Task(ws, Solvers[AvailableSolverTypes.BaseSolver]));
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
                Logger.UltraLogger.Instance.AddToLog("OTIWIterator: Wrong init params.", Logger.MsgType.Error);
                throw new NotImplementedException();
            }
            Blocks = new List<InputsIteratorBlock>(this.BlocksCount);
            Blocks.AddRange(Enumerable.Repeat(new InputsIteratorBlock(this.BlockLength), this.BlocksCount));
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
            throw new NotImplementedException();
        }
    }
    class InputsIteratorBlock
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
}
