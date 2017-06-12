﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Allax
{
    public struct SolverParams
    {
        public SolverParams(SPNetWay Way, ISPNet Net, AnalisysType Type, int MaxActiveBlocksOnLayer)
        {
            this.Net = Net;
            this.Type=Type;
            this.Way = Way;
            this.P = new Prevalence(0, 0, Net.GetSettings().word_length / Net.GetSettings().sblock_count);
            this.MaxActiveBlocksOnLayer = MaxActiveBlocksOnLayer;
        }
        public SPNetWay Way;
        public Prevalence P;
        public int MaxActiveBlocksOnLayer;
        public AnalisysType Type;
        public ISPNet Net;
    }
}
