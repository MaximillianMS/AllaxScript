using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Allax
{
    public struct SolverParams
    {
        public SolverParams(SPNetWay Way, IEngine Engine, AnalisysType Type, int MaxActiveBlocksOnLayer)
        {
            this.Engine = Engine;
            this.Type=Type;
            this.Way = Way;
            this.P = new Prevalence(0, 0, Engine.GetSPNetInstance().GetSettings().SBoxSize);
            this.MaxActiveBlocksOnLayer = MaxActiveBlocksOnLayer;
            this.BIndex = -1;
            this.lastNotEmptyLayerIndex = -1;
        }
        public int BIndex;
        public int lastNotEmptyLayerIndex;
        public SPNetWay Way;
        public Prevalence P;
        public int MaxActiveBlocksOnLayer;
        public AnalisysType Type;
        public IEngine Engine;
    }
}
