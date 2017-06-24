using System;
using System.Collections.Generic;

namespace Allax
{
    class GreedySolver : BaseSolver
    {
        protected override int GetStatesCount(List<BlockState> States, SolverParams SolParams)
        {
            if (SolParams.lastNotEmptyLayerIndex > SolParams.Way.layers.Count - 6 && States.Count != 0)
            {
                var ret = 0;
                var max = Math.Abs(States[0].MatrixValue);
                for (; ret < States.Count; ret++)
                {
                    if (Math.Abs(States[ret].MatrixValue) < max)
                        break;
                }
                return ret + 1;
            }
            else
                return (States.Count == 0) ? 0 : 1;
        }
    }
}
