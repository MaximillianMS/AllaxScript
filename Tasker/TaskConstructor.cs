using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Allax
{
    public struct MultiTime
    {
        Int64 UnixTime;
        Int64 Ticks;
    }
    public struct SolverParams
    {
        int Weight;
        MultiTime Time;
    }
    public struct Task
    {
        SPNetWay Way;
        SolverParams Params;
        public Task(SPNetWay Way, SolverParams Params)
        {
            this.Way = Way;
            this.Params = Params;
        }
    }
    public class TaskConstructor
    {
        int _rounds_count;
        Int64 MIN;
        ISPNet _net;
        Queue<Task> _tasks;
        SPNetWay _tempEmptyWay;
        OpenTextInputWeightIterator Iter;
        public TaskConstructor(ISPNet Net)
        {
            _net = Net;
            _rounds_count = _net.GetLayers().Count / 3;
            _tempEmptyWay = WayConverter.ToEmptyWay(_net);
        }
        void AnalysePreviousTasks()
        {
            throw new NotImplementedException();
        }
        public Queue<Task> GetTasks(int count)
        {
            if (_tasks.Count != 0)
            {
                AnalysePreviousTasks();
            }
            _tasks.Clear();
            for (int i = 0; (i < count) && Iter.IsFinished(); i++)
            {
                OpenTextInput NextInput = Iter.NextState();
                SPNetWay ws = WayConverter.ToWay(_net, NextInput);
                _tasks.Enqueue(new Task(ws, new SolverParams()));
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
        List<byte> input;
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
        List<byte> input;
        int weight;
    }
}
