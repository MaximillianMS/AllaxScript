using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Allax
{
    public struct SolverParams
    {
        public SolverParams(SPNetWay Way, ISPNet Net, AnalisysType Type, int MaxActiveBlocksOnLayer/*, CallbackAddTask AddTaskFunc*/)
        {
            this.Net = Net;
            this.Type=Type;
            //this.AddTaskFunc = AddTaskFunc;
            this.Way = Way;
            this.P = new Prevalence(0, 0, Way.layers[1].blocks[0].active_inputs.Count);
            this.MaxActiveBlocksOnLayer = MaxActiveBlocksOnLayer;
        }
        public SPNetWay Way;
        public Prevalence P;
        public int MaxActiveBlocksOnLayer;
        public AnalisysType Type;
        public ISPNet Net;
        //public CallbackAddTask AddTaskFunc;
    }
}
