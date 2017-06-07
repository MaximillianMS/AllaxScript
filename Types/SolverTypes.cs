using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Allax
{
    public struct SolverParams
    {
        public SolverParams(ISPNet Net, AnalisysType Type, CallbackAddTask AddTaskFunc)
        {
            this.Net = Net;
            this.Type = Type;
            this.AddTaskFunc = AddTaskFunc;
        }
        public AnalisysType Type;
        public ISPNet Net;
        public CallbackAddTask AddTaskFunc;
    }
}
