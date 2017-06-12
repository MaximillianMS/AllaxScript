using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Allax
{
    //almost static
    public class BaseSolver:ISolver
    {
        protected virtual void KLayer(SolverParams SolParams, int LIndex)
        {
            var kblock = SolParams.Way.layers[LIndex].blocks[0];
            kblock.active_outputs = kblock.active_inputs;
            SolParams.Way.layers[LIndex].blocks[0] = kblock;
        }
        protected virtual bool SLayer(SolverParams SolParams, int LIndex, int BIndex=0)
        {
            bool ret = true; //just checking mode
            int ActiveBlocksCount = 0;
            for (; BIndex < SolParams.Way.layers[LIndex].blocks.Count; BIndex++)
            {
                var WayBlock = SolParams.Way.layers[LIndex].blocks[BIndex];
                if (WayBlock.active_inputs.All(x=>!x))
                {
                    continue;//clear inputs, so this block was not chosen, block is not active
                }
                if (!WayBlock.active_outputs.All(x => !x))
                {
                    ActiveBlocksCount++;
                    continue; //skip already solved block
                }
                ret = false;//producing new solves mode
                if (ActiveBlocksCount + 1 > SolParams.MaxActiveBlocksOnLayer)
                {
                    break;
                }
                var NetBlock = SolParams.Net.GetLayers()[LIndex].GetBlocks()[BIndex];
                var Params = new BlockStateExtrParams(WayBlock.active_inputs, null, SolParams.Net.GetMultiThreadPrevalence(), SolParams.P, SolParams.Type, true);
                var States = NetBlock.ExtractStates(Params);
                for(int i=0;i<States.Count;i++)
                {
                    var State = States[i];
                    var NewWay = WayConverter.CloneWay(SolParams.Way);
                    var NewBLock = NewWay.layers[LIndex].blocks[BIndex];
                    NewBLock.active_outputs = State._outputs;
                    NewWay.layers[LIndex].blocks[BIndex] = NewBLock;
                    var NewSolParams = SolParams;
                    NewSolParams.P *= State.MatrixValue;
                    NewSolParams.Way = NewWay;
                    Solve(NewSolParams);
                }
                break;
            }
            return ret;
        }
        protected virtual void PLayer(SolverParams SolParams, int LIndex)
        {
            var Params = new BlockStateExtrParams(SolParams.Way.layers[LIndex].blocks[0].active_inputs, null,
                                                                new Prevalence(), new Prevalence(), SolParams.Type);
            var States = SolParams.Net.GetLayers()[LIndex].GetBlocks()[0].ExtractStates(Params);
            if (States.Count == 1)
            {
                //deep copying
                foreach (var j in Enumerable.Range(0, SolParams.Way.layers[LIndex].blocks[0].active_outputs.Count))
                {
                   SolParams.Way.layers[LIndex].blocks[0].active_outputs[j] = States[0]._outputs[j];
                }
            }
            else
            {
                Logger.UltraLogger.Instance.AddToLog("BaseSolver: Cant extract state from P-layer", Logger.MsgType.Error);
                throw new NotImplementedException();
            }
        }
        public virtual void Solve(SolverParams SolParams)
        {
            if(WayConverter.ToLong(SolParams.Way.layers[0].blocks[0].active_inputs)==15)
            {
                ;
            }
            var layersCount = SolParams.Way.layers.Count();
            var roundsCount = layersCount / 3;
            #region FindLastNotEmptyLayer
            int lastNotEmptyLayerIndex = WayConverter.SearchLastNotEmptyLayer(SolParams.Way);
            #endregion
            #region FullRounds
            if ((lastNotEmptyLayerIndex >= 0) && (lastNotEmptyLayerIndex / 3 < roundsCount - 1))
            {
                for (int i = lastNotEmptyLayerIndex / 3; i < roundsCount - 1; i++)
                {
                    #region K-layer
                    if (SolParams.Way.layers[lastNotEmptyLayerIndex].type==LayerType.KLayer)
                    {
                        KLayer(SolParams, lastNotEmptyLayerIndex);
                        WayConverter.CopyOutToIn(SolParams.Way, lastNotEmptyLayerIndex, lastNotEmptyLayerIndex + 1);
                        lastNotEmptyLayerIndex++;
                    }
                    #endregion
                    #region S-layer
                    if (SolParams.Way.layers[lastNotEmptyLayerIndex].type == LayerType.SLayer)
                    {
                        if (!(SLayer(SolParams, lastNotEmptyLayerIndex)))
                        {
                            return;
                        }
                        WayConverter.CopyOutToIn(SolParams.Way, lastNotEmptyLayerIndex, lastNotEmptyLayerIndex + 1);
                        lastNotEmptyLayerIndex++;
                    }
                    #endregion
                    #region P-layer
                    if (SolParams.Way.layers[lastNotEmptyLayerIndex].type == LayerType.PLayer)
                    {
                        PLayer(SolParams, lastNotEmptyLayerIndex);
                        WayConverter.CopyOutToIn(SolParams.Way, lastNotEmptyLayerIndex, lastNotEmptyLayerIndex + 1);
                        lastNotEmptyLayerIndex++;
                    }
                    #endregion
                }
            }
            #endregion
            #region LastRound
            //No need to process LastRound, because LastRound must be reversed.
            #endregion
            SolParams.Net.GetCallbackAddSolution()(new Solution(SolParams.P, SolParams.Way));
            if (SolParams.P > SolParams.Net.GetMultiThreadPrevalence())
            {
                SolParams.Net.SetMultiThreadPrevalence(SolParams.P);
            }
        }
    }
}
