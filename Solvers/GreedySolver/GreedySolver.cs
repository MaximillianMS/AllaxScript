using System;
using System.Collections.Generic;

namespace Allax
{
    class GreedySolver : BaseSolver
    {
        protected override int GetStatesCount(List<BlockState> States, SolverParams SolParams)
        {
            if (/*SolParams.lastNotEmptyLayerIndex > SolParams.Way.layers.Count - 6 &&*/ States.Count != 0)
            {
                return GetMaxStates(States);
            }
            else
                return (States.Count == 0) ? 0 : 1;
        }
    }
}
