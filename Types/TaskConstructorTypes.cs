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
    public struct ExtraParams
    {
        int Weight;
        MultiTime Time;
    }
    public struct Task
    {
        SPNetWay Way;
        ExtraParams Params;
        public Task(SPNetWay Way, ExtraParams Params)
        {
            this.Way = Way;
            this.Params = Params;
        }
    }
    public struct TaskConstructorParams
    {
        public TaskConstructorParams(ISPNet Net)
        {
            this.Net = Net;
        }
        public ISPNet Net;
    }
}
