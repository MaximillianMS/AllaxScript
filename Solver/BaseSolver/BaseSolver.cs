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
        SolverParams Params;
        public void Init(SolverParams Params)
        {
            this.Params = Params;
        }
        /// <summary>
        /// Automatically calls "init" func
        /// </summary>
        /// <param name="Params"></param>
        public BaseSolver(SolverParams Params)
        {
            Init(Params);
        }
        void KLayer(SPNetWay Way, int LIndex)
        {
            var kblock = Way.layers[LIndex].blocks[0];
            kblock.active_outputs = kblock.active_inputs;
            Way.layers[LIndex].blocks[0] = kblock;
        }
        void SLayer(SPNetWay Way, int LIndex, ref Prevalence CurrentPrevalence, int BIndex=0)
        {
            for (; BIndex < Way.layers[LIndex].blocks.Count; BIndex++)
            {
                var WayBlock = Way.layers[LIndex].blocks[BIndex];
                if (WayBlock.active_inputs.All(x=>!x))
                {
                    continue;//clear inputs, so this block was not chosen, block is not active
                }
                if (!WayBlock.active_outputs.All(x => !x))
                {
                    continue; //skip already solved block
                }
                var NetBlock = this.Params.Net.GetLayers()[LIndex].GetBlocks()[BIndex];
                var Params = new BlockStateExtrParams(WayBlock.active_inputs, null, this.Params.Net.GetMultiThreadPrevalence(), CurrentPrevalence, true);
                var States = NetBlock.ExtractStates(Params);
                for(int i=0;i<States.Count;i++)
                {
                    var State = States[i];
                    var NewWay = WayConverter.CloneWay(Way);
                    var NewBLock = NewWay.layers[LIndex].blocks[BIndex];
                    NewBLock.active_outputs = State._outputs;
                    NewWay.layers[LIndex].blocks[BIndex] = NewBLock;
                    CurrentPrevalence *= State.MatrixValue;
                    NewWay.layers[LIndex].blocks[BIndex] = WayBlock;
                    Solve(NewWay, CurrentPrevalence);
                }
            }

        }
        void PLayer(SPNetWay Way, int LIndex)
        {
            var Params = new BlockStateExtrParams(Way.layers[LIndex].blocks[0].active_inputs, null,
                                                                new Prevalence(), new Prevalence());
            var States = this.Params.Net.GetLayers()[LIndex].GetBlocks()[0].ExtractStates(Params);
            if (States.Count == 1)
            {
                //deep copying
                foreach (var j in Enumerable.Range(0, Way.layers[LIndex].blocks[0].active_outputs.Count))
                {
                    Way.layers[LIndex].blocks[0].active_outputs[j] = States[0]._outputs[j];
                }
            }
            else
            {
                Logger.UltraLogger.Instance.AddToLog("BaseSolver: Cant extract state from P-layer", Logger.MsgType.Error);
                throw new NotImplementedException();
            }
        }
        public void Solve(SPNetWay Way, Prevalence CurrentPrevalence)
        {
            var layersCount = Way.layers.Count();
            var roundsCount = layersCount / 3;
            #region FindLastNotEmptyLayer
            int lastNotEmptyLayerIndex = WayConverter.SearchLastNotEmptyLayer(Way);
            #endregion
            #region FullRounds
            if ((lastNotEmptyLayerIndex >= 0) && (lastNotEmptyLayerIndex / 3 < roundsCount - 1))
            {
                for (int i = lastNotEmptyLayerIndex / 3; i < roundsCount - 1; i++)
                {
                    #region K-layer
                    if (Way.layers[i].type==LayerType.KLayer)
                    {
                        KLayer(Way, lastNotEmptyLayerIndex);
                        WayConverter.CopyOutToIn(Way, lastNotEmptyLayerIndex, lastNotEmptyLayerIndex + 1);
                        lastNotEmptyLayerIndex++;
                    }
                    #endregion
                    #region S-layer
                    if (Way.layers[i].type == LayerType.SLayer)
                    {
                        SLayer(Way, lastNotEmptyLayerIndex, ref CurrentPrevalence);
                        WayConverter.CopyOutToIn(Way, lastNotEmptyLayerIndex, lastNotEmptyLayerIndex + 1);
                        lastNotEmptyLayerIndex++;
                    }
                    #endregion
                    #region P-layer
                    if (Way.layers[i].type == LayerType.PLayer)
                    {
                        PLayer(Way, lastNotEmptyLayerIndex);
                        WayConverter.CopyOutToIn(Way, lastNotEmptyLayerIndex, lastNotEmptyLayerIndex + 1);
                        lastNotEmptyLayerIndex++;
                    }
                    #endregion
                }
            }
            #endregion
            #region LastRound
            //No need to process LastRound, because LastRound must be reversed.
            #endregion
            Params.Net.GetCallbackAddSolution()(new Solution(CurrentPrevalence, Way));
        }
    }
}
