using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Allax
{
    public interface ISolver
    {
        void Init(SolverParams Params);
        void Solve(SPNetWay Way, Prevalence CurrentPrevalence);
    }
}
