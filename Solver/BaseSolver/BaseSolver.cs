using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Allax
{
    public static class BaseSolver
    {
        static int SearchLastNotEmptyLayer(SPNetWay Way)
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
        static void CopyOutToIn(SPNetWay Way, int SrcLIndex, int DestLIndex)
        {
            throw new NotImplementedException();
            if(SrcLIndex<Way.layers.Count&&DestLIndex<Way.layers.Count)
            {
                #region From S-layer to P-layer
                if (Way.layers[SrcLIndex].type == LayerType.SLayer && Way.layers[DestLIndex].type == LayerType.PLayer)
                {
                    Way.layers[DestLIndex].blocks[0].active_inputs.Clear();
                    for (int i = 0; i < Way.layers[SrcLIndex].blocks.Count; i++)
                    {
                        for (int j = 0; j < Way.layers[SrcLIndex].blocks[i].active_outputs.Count; j++)
                        {
                            Way.layers[DestLIndex].blocks[0].active_inputs.Add(Way.layers[SrcLIndex].blocks[i].active_inputs[j]);
                        }
                    }
                }
                #endregion
                #region From P-Layer to K-Layer
                if (Way.layers[SrcLIndex].type == LayerType.PLayer && Way.layers[DestLIndex].type == LayerType.KLayer)
                {
                    var block = Way.layers[DestLIndex].blocks[0];
                    block.active_inputs = Way.layers[SrcLIndex].blocks[0].active_outputs;
                    Way.layers[DestLIndex].blocks[0] = block;
                }
                #endregion
                #region From K-Layer to S-Layer
                if(Way.layers[SrcLIndex].type == LayerType.KLayer && Way.layers[DestLIndex].type == LayerType.SLayer)
                {
                    var srcBIndex = 0;
                    for (int i = 0; i < Way.layers[DestLIndex].blocks.Count; i++)
                    {
                        for (int j = 0; (j < Way.layers[DestLIndex].blocks[i].active_outputs.Count)&&(srcBIndex<Way.layers[srcBIndex].blocks[0].active_outputs.Count); j++, srcBIndex++)
                        {
                            Way.layers[DestLIndex].blocks[i].active_inputs[j]=Way.layers[SrcLIndex].blocks[0].active_outputs[srcBIndex];
                        }
                    }
                }
            }
            else
            {
                Logger.UltraLogger.Instance.AddToLog("BaseSolver: Copying layers error.", Logger.MsgType.Error);
            }
        }
        static void KLayer(SPNetWay Way, int LIndex)
        {
            var kblock = Way.layers[LIndex].blocks[0];
            kblock.active_outputs = kblock.active_inputs;
            Way.layers[LIndex].blocks[0] = kblock;
        }
        static void SLayer(SPNetWay Way, ISPNet Net, int LIndex, ref long MIN, ref long CurrentCor)
        {
            throw new NotImplementedException();
            for (int BIndex = 0; BIndex < Way.layers[LIndex].blocks.Count; BIndex++)
            {
                var WayBlock = Way.layers[LIndex].blocks[BIndex];
                var NetBlock = Net.GetLayers()[LIndex].GetBlock((byte)BIndex);
                var States = NetBlock.ExtractStates(new BlockStateExtrParams(WayBlock.active_inputs, null, MIN, CurrentCor, true));
                foreach (var State in States)
                {
                    var Outputs = State._outputs;
                    WayBlock.active_outputs = Outputs;
                    CurrentCor *= State._cor;
                    Way.layers[LIndex].blocks[BIndex] = WayBlock;
                    //Solver(Net, Way, ref MIN, CurrentCor);
                }
            }

        }
        static void PLayer(SPNetWay Way, ISPNet Net, int LIndex)
        {
            var Params = new BlockStateExtrParams(Way.layers[LIndex].blocks[0].active_inputs, null, 0, 0, false);
            var States = Net.GetLayers()[LIndex].GetBlocks()[0].ExtractStates(Params);
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
        public static void Solver(ISPNet Net, SPNetWay Way, ref long MIN, long CurrentCor)
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
                        CopyOutToIn(Way, lastNotEmptyLayerIndex, lastNotEmptyLayerIndex + 1);
                        lastNotEmptyLayerIndex++;
                    }
                    #endregion
                    #region S-layer
                    if (Way.layers[i].type == LayerType.SLayer)
                    {
                        SLayer(Way, Net, lastNotEmptyLayerIndex, ref MIN, ref CurrentCor);
                        CopyOutToIn(Way, lastNotEmptyLayerIndex, lastNotEmptyLayerIndex + 1);
                        lastNotEmptyLayerIndex++;
                    }
                    #endregion
                    #region P-layer
                    if (Way.layers[i].type == LayerType.PLayer)
                    {
                        PLayer(Way, Net, lastNotEmptyLayerIndex);
                        CopyOutToIn(Way, lastNotEmptyLayerIndex, lastNotEmptyLayerIndex + 1);
                        lastNotEmptyLayerIndex++;
                    }
                    #endregion
                }
            }
            #endregion
            #region LastRound
            throw new NotImplementedException();
            #endregion
        }
    }
}
