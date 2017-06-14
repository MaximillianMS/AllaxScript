using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Allax
{
    class HeuristicSolver : BaseSolver
    {
        protected override bool SLayer(SolverParams SolParams)
        {
            //var LIndex = SolParams.lastNotEmptyLayerIndex;
            var ret = true;
            int ActiveBlocksCount = 0;
            //var BIndex = SolParams.BIndex;
            if(SolParams.BIndex == -1)
            {
                SolParams.BIndex = 0;
                for (; SolParams.BIndex < SolParams.Way.layers[SolParams.lastNotEmptyLayerIndex].blocks.Count; SolParams.BIndex++)
                {
                    var WayBlock = SolParams.Way.layers[SolParams.lastNotEmptyLayerIndex].blocks[SolParams.BIndex];
                    if (!WayBlock.active_inputs.All(x => !x))
                    {
                        ActiveBlocksCount++;
                        continue;
                    }
                }
                if (ActiveBlocksCount > SolParams.MaxActiveBlocksOnLayer)
                {
                    return false;
                }
                SolParams.BIndex = 0;
            }
            for (; SolParams.BIndex < SolParams.Way.layers[SolParams.lastNotEmptyLayerIndex].blocks.Count; SolParams.BIndex++)
            {
                var WayBlock = SolParams.Way.layers[SolParams.lastNotEmptyLayerIndex].blocks[SolParams.BIndex];
                if (WayBlock.active_inputs.All(x => !x))
                {
                    continue;
                }
                if (!WayBlock.active_outputs.All(x => !x))
                {
                    //ActiveBlocksCount++;
                    continue; //already solved block
                }
                ret = false;
                var NetBlock = SolParams.Net.GetLayers()[SolParams.lastNotEmptyLayerIndex].GetBlocks()[SolParams.BIndex];
                var Params = new BlockStateExtrParams(WayBlock.active_inputs, null, SolParams.Net.GetMultiThreadPrevalence(), SolParams.P, SolParams.Type, true);
                var States = NetBlock.ExtractStates(Params);
                if(States.Count>0)
                {
                    var State = States[0];
                    var NewWay = WayConverter.CloneWay(SolParams.Way);
                    var NewBLock = NewWay.layers[SolParams.lastNotEmptyLayerIndex].blocks[SolParams.BIndex];
                    NewBLock.active_outputs = State._outputs;
                    NewWay.layers[SolParams.lastNotEmptyLayerIndex].blocks[SolParams.BIndex] = NewBLock;
                    var NewSolParams = SolParams;
                    NewSolParams.P *= State.MatrixValue;
                    NewSolParams.Way = NewWay;
                    NewSolParams.BIndex++;
                    Solve(NewSolParams);
                }
                break;
            }
            return ret;
        }
    }
}
