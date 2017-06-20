using System.Collections.Generic;

namespace Allax
{
    class GreedySolver : BaseSolver
    {
        protected override int GetStatesCount(List<BlockState> States, SolverParams SolParams)
        {
            return (States.Count == 0) ? 0 : 1;
        }
    }
}
