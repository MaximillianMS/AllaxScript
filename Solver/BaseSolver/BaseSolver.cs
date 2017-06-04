using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Allax
{
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
        int SearchLastNotEmptyLayer(SPNetWay Way)
        {
            int index = -1;
            for (int i = 0; i < Way.layers.Count; i++)
            {
                int input_sum = 0;
                if (Way.layers[i].blocks != null)
                    foreach (var block in Way.layers[i].blocks)
                    {
                        if (block.active_inputs != null)
                            foreach (var input in block.active_inputs)
                            {
                                input_sum += Convert.ToInt32(input);
                            }
                    }
                if (input_sum == 0)
                {
                    index = i - 1;
                }
            }
            return index;
        }
        void KLayer(SPNetWay Way, int LIndex)
        {
            var kblock = Way.layers[LIndex].blocks[0];
            kblock.active_outputs = kblock.active_inputs;
            Way.layers[LIndex].blocks[0] = kblock;
        }
        void SLayer(SPNetWay Way, int LIndex, ref long CurrentCor)
        {
            throw new NotImplementedException();
            for (int BIndex = 0; BIndex < Way.layers[LIndex].blocks.Count; BIndex++)
            {
                var WayBlock = Way.layers[LIndex].blocks[BIndex];
                var NetBlock = this.Params.Net.GetLayers()[LIndex].GetBlock((byte)BIndex);
                var Params = new BlockStateExtrParams(WayBlock.active_inputs, null, this.Params.Net.GetMultiThreadMIN(), CurrentCor, true);
                var States = NetBlock.ExtractStates(Params);
                foreach (var State in States)
                {
                    var NewWay = WayConverter.CloneWay(Way);
                    var Outputs = State._outputs;
                    WayBlock.active_outputs = Outputs;
                    CurrentCor *= State._cor;
                    NewWay.layers[LIndex].blocks[BIndex] = WayBlock;
                    //Solver(Net, Way, ref MIN, CurrentCor);
                }
            }

        }
        void PLayer(SPNetWay Way, int LIndex)
        {
            var Params = new BlockStateExtrParams(Way.layers[LIndex].blocks[0].active_inputs, null, 0, 0, false);
            var States = this.Params.Net.GetLayers()[LIndex].GetBlocks()[0].ExtractStates(Params);
            if(States.Count==1)
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
        public void Solve(SPNetWay Way, long CurrentCor)
        {
            throw new NotImplementedException();
            var layersCount = Way.layers.Count();
            var roundsCount = layersCount / 3;
            #region FindLastNotEmptyLayer
            int lastNotEmptyLayerIndex = SearchLastNotEmptyLayer(Way);
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
                        SLayer(Way, lastNotEmptyLayerIndex, ref CurrentCor);
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
            throw new NotImplementedException();
            #endregion
            this.Params.Net.GetCallbackAddSolution()(new Solution());
        }
    }
}
