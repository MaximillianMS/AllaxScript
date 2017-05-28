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
}
