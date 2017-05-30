using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Allax
{
    public delegate void CallbackAddTask(Task T);
    public interface ITaskConstructor
    {
        ConcurrentQueue<Task> GetTasks(int Count);
        void AddTask(Task T);
        void Init(TaskConstructorParams Params);
    }
    public class TaskConstructor:ITaskConstructor
    {
        private object syncRoot = new object();
        int _rounds_count;
        ISPNet _net;
        ConcurrentQueue<Task> _tasks;
        SPNetWay _tempEmptyWay;
        OpenTextInputWeightIterator Iter;
        ISolver Solver;
        public TaskConstructor(TaskConstructorParams Params)
        {
            Init(Params);
            _rounds_count = _net.GetLayers().Count / 3;
            _tempEmptyWay = WayConverter.ToWay(_net);

            Solver = new BaseSolver(new SolverParams(_net, AddTask));
        }
        public void AddTask(Task T)
        {
            _tasks.Enqueue(T);
        }
        public void Init(TaskConstructorParams Params)
        {
            _net = Params.Net;
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
            _tasks = new ConcurrentQueue<Task>();
            for (int i = 0; (i < count) && Iter.IsFinished(); i++)
            {
                OpenTextInput NextInput = Iter.NextState();
                SPNetWay ws = WayConverter.ToWay(_net, NextInput);
                _tasks.Enqueue(new Task(ws, new ExtraParams(Solver)));
            }
            return _tasks;
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
