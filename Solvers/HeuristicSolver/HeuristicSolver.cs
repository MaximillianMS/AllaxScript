using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Allax
{
    class HeuristicSolver : BaseSolver
    {
        protected override bool SLayer(SolverParams SolParams, int LIndex, int BIndex = 0)
        {
            var ret = true;
            int ActiveBlocksCount = 0;
            for (; BIndex < SolParams.Way.layers[LIndex].blocks.Count; BIndex++)
            {
                var WayBlock = SolParams.Way.layers[LIndex].blocks[BIndex];
                if (WayBlock.active_inputs.All(x => !x))
                {
                    continue;
                }
                if (!WayBlock.active_outputs.All(x => !x))
                {
                    ActiveBlocksCount++;
                    continue; //already solved block
                }
                ret = false;
                if (ActiveBlocksCount + 1 > SolParams.MaxActiveBlocksOnLayer)
                {
                    break;
                }
                var NetBlock = SolParams.Net.GetLayers()[LIndex].GetBlocks()[BIndex];
                var Params = new BlockStateExtrParams(WayBlock.active_inputs, null, SolParams.Net.GetMultiThreadPrevalence(), SolParams.P, SolParams.Type, true);
                var States = NetBlock.ExtractStates(Params);
                if(States.Count>0)
                {
                    var State = States[0];
                    var NewWay = WayConverter.CloneWay(SolParams.Way);
                    var NewBLock = NewWay.layers[LIndex].blocks[BIndex];
                    NewBLock.active_outputs = State._outputs;
                    NewWay.layers[LIndex].blocks[BIndex] = NewBLock;
                    SolParams.P *= State.MatrixValue;
                    SolParams.Way = NewWay;
                    Solve(SolParams);
                }
            }
            return ret;
        }
    }
}
