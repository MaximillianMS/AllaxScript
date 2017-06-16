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
        protected virtual void KLayer(SolverParams SolParams)
        {
            var LIndex = SolParams.lastNotEmptyLayerIndex;
            var kblock = SolParams.Way.layers[LIndex].blocks[0];
            kblock.active_outputs = kblock.active_inputs;
            SolParams.Way.layers[LIndex].blocks[0] = kblock;
        }
        protected virtual bool SLayer(SolverParams SolParams)
        {
            bool ret = true; //just checking mode
            int ActiveBlocksCount = 0;
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
                    continue; //already solved block
                }
                ret = false;
                var NetBlock = SolParams.Engine.GetSPNetInstance().GetLayers()[SolParams.lastNotEmptyLayerIndex].GetBlocks()[SolParams.BIndex];
                var Params = new BlockStateExtrParams(WayBlock.active_inputs, null, SolParams.Engine.GetMultiThreadPrevalence(), SolParams.P, SolParams.Type, true);
                var States = NetBlock.ExtractStates(Params);
                for (int i=0;i<States.Count;i++)
                {
                    var State = States[i];
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
        protected virtual void PLayer(SolverParams SolParams)
        {
            var LIndex = SolParams.lastNotEmptyLayerIndex;
            var Params = new BlockStateExtrParams(SolParams.Way.layers[LIndex].blocks[0].active_inputs, null,
                                                                new Prevalence(), new Prevalence(), SolParams.Type);
            var States = SolParams.Engine.GetSPNetInstance().GetLayers()[LIndex].GetBlocks()[0].ExtractStates(Params);
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
            var layersCount = SolParams.Way.layers.Count();
            var roundsCount = layersCount / 3;
            #region FindLastNotEmptyLayer
            if (SolParams.lastNotEmptyLayerIndex == -1)
                SolParams.lastNotEmptyLayerIndex = WayConverter.SearchLastNotEmptyLayer(SolParams.Way);
            #endregion
            #region FullRounds
            if ((SolParams.lastNotEmptyLayerIndex >= 0) && (SolParams.lastNotEmptyLayerIndex / 3 < roundsCount - 1))
            {
                for (int i = SolParams.lastNotEmptyLayerIndex / 3; i < roundsCount - 1; i++)
                {
                    #region K-layer
                    if (SolParams.Way.layers[SolParams.lastNotEmptyLayerIndex].type==LayerType.KLayer)
                    {
                        KLayer(SolParams);
                        WayConverter.CopyOutToIn(SolParams.Way, SolParams.lastNotEmptyLayerIndex, SolParams.lastNotEmptyLayerIndex + 1);
                        SolParams.lastNotEmptyLayerIndex++;
                        SolParams.BIndex = -1;
                    }
                    #endregion
                    #region S-layer
                    if (SolParams.Way.layers[SolParams.lastNotEmptyLayerIndex].type == LayerType.SLayer)
                    {
                        if (!(SLayer(SolParams)))
                        {
                            return;
                        }
                        WayConverter.CopyOutToIn(SolParams.Way, SolParams.lastNotEmptyLayerIndex, SolParams.lastNotEmptyLayerIndex + 1);
                        SolParams.lastNotEmptyLayerIndex++;
                    }
                    #endregion
                    #region P-layer
                    if (SolParams.Way.layers[SolParams.lastNotEmptyLayerIndex].type == LayerType.PLayer)
                    {
                        PLayer(SolParams);
                        WayConverter.CopyOutToIn(SolParams.Way, SolParams.lastNotEmptyLayerIndex, SolParams.lastNotEmptyLayerIndex + 1);
                        SolParams.lastNotEmptyLayerIndex++;
                    }
                    #endregion
                }
            }
            #endregion
            #region LastRound
            //No need to process LastRound, because LastRound must be reversed.
            #endregion
            var Net = SolParams.Engine.GetSPNetInstance();
            SolParams.Engine.GetSettings().AddSolution(new Solution(SolParams.P, SolParams.Way));
            if (SolParams.P > SolParams.Engine.GetMultiThreadPrevalence())
            {
                SolParams.Engine.SetMultiThreadPrevalence(SolParams.P);
            }
        }
    }
}
