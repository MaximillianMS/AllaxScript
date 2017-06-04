using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Allax
{
    public static class WayConverter
    {
        //need to implement. True as default.
        public static bool CheckStandartNetCondition(ISPNet Net)
        {
            return true;
        }
        public static SPNetWay ToWay(ISPNet Net)
        {
            var NetWay = new SPNetWay();
            if (CheckStandartNetCondition(Net))
            {
                var Layers = Net.GetLayers();
                var Settings = Net.GetSettings();
                var LayerCount = Layers.Count;
                NetWay.layers = new List<SPNetWayLayer>();
                for (int i = 0; i < LayerCount; i++)
                {
                    var tmp = new SPNetWayLayer();
                    tmp.type = Layers[i].GetLayerType();
                    if (tmp.type != LayerType.SLayer)
                    {
                        tmp.blocks = new List<SPNetWayBlock>();
                        var tmp_block = new SPNetWayBlock();
                        tmp_block.active_inputs = new List<bool>();
                        tmp_block.active_outputs = new List<bool>();
                        tmp_block.active_inputs.AddRange(Enumerable.Repeat(false, Settings.word_length));
                        tmp_block.active_outputs.AddRange(Enumerable.Repeat(false, Settings.word_length));
                        tmp.blocks.Add(tmp_block);
                    }
                    else
                    {
                        foreach (var j in Enumerable.Range(0, Settings.sblock_count))
                        {
                            var tmp_sblock = new SPNetWayBlock();
                            tmp_sblock.active_inputs = new List<bool>();
                            tmp_sblock.active_outputs = new List<bool>();
                            tmp_sblock.active_inputs.AddRange(Enumerable.Repeat(false, Settings.word_length / Settings.sblock_count));
                            tmp_sblock.active_outputs.AddRange(Enumerable.Repeat(false, Settings.word_length / Settings.sblock_count));
                            tmp.blocks.Add(tmp_sblock);
                        }
                    }
                    NetWay.layers.Add(tmp);

                }
                return NetWay;
            }
            else
            {
                throw new Exception("Azaza. Wrong net.");
            }
        }
        /// <summary>
        /// Deep Clone
        /// </summary>
        /// <param name="Way"></param>
        /// <param name="empty">Copy just struct without values</param>
        /// <returns></returns>
        public static SPNetWay CloneWay(SPNetWay Way, bool empty = false)
        {
            SPNetWay NewWay = new SPNetWay();
            if (Way.layers != null)
            {
                NewWay.layers = new List<SPNetWayLayer>(Way.layers.Count);
                for (int i = 0; i < Way.layers.Count; i++)
                {
                    var tmp_layer = new SPNetWayLayer();
                    tmp_layer.type = Way.layers[i].type;
                    if (Way.layers[i].blocks != null)
                    {
                        for (int j = 0; j < Way.layers[i].blocks.Count; j++)
                        {
                            var tmp_block = new SPNetWayBlock();
                            if (Way.layers[i].blocks[j].active_inputs != null & Way.layers[i].blocks[j].active_outputs != null)
                            {
                                tmp_block.active_inputs = new List<bool>(Way.layers[i].blocks[j].active_inputs.Count);
                                tmp_block.active_outputs = new List<bool>(Way.layers[i].blocks[j].active_outputs.Count);
                                tmp_block.active_inputs.AddRange(Enumerable.Repeat(false, Way.layers[i].blocks[j].active_inputs.Count));
                                tmp_block.active_outputs.AddRange(Enumerable.Repeat(false, Way.layers[i].blocks[j].active_outputs.Count));
                                if (!empty & (Way.layers[i].blocks[j].active_inputs.Count == Way.layers[i].blocks[j].active_outputs.Count))
                                {
                                    for (int k = 0; k < Way.layers[i].blocks[j].active_inputs.Count; k++)
                                    {
                                        tmp_block.active_inputs[k] = Way.layers[i].blocks[j].active_inputs[k];
                                        tmp_block.active_outputs[k] = Way.layers[i].blocks[j].active_outputs[k];
                                    }
                                }

                            }
                            tmp_layer.blocks.Add(tmp_block);
                        }
                    }
                    NewWay.layers.Add(tmp_layer);
                }
            }
            return NewWay;
        }
        public static SPNetWay ToWay(ISPNet Net, OpenTextInput Input)
        {
            var Way = ToWay(Net);
            foreach(var j in Enumerable.Range(0, Input.input.Count))
            {
                Way.layers[0].blocks[0].active_inputs[j] = Input.input[j];
            }
            return Way;
        }
        public static void CopyOutToIn(SPNetWay Way, int SrcLIndex, int DestLIndex)
        {
            throw new NotImplementedException();
            if (SrcLIndex < Way.layers.Count && DestLIndex < Way.layers.Count)
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
                if (Way.layers[SrcLIndex].type == LayerType.KLayer && Way.layers[DestLIndex].type == LayerType.SLayer)
                {
                    var srcBIndex = 0;
                    for (int i = 0; i < Way.layers[DestLIndex].blocks.Count; i++)
                    {
                        for (int j = 0; (j < Way.layers[DestLIndex].blocks[i].active_outputs.Count) && (srcBIndex < Way.layers[srcBIndex].blocks[0].active_outputs.Count); j++, srcBIndex++)
                        {
                            Way.layers[DestLIndex].blocks[i].active_inputs[j] = Way.layers[SrcLIndex].blocks[0].active_outputs[srcBIndex];
                        }
                    }
                }
                #endregion
            }
            else
            {
                Logger.UltraLogger.Instance.AddToLog("WayConverter: CopyOutToIn Func. I cant copy from and to this layer types.", Logger.MsgType.Error);
            }
        }
        public static long ToLong(List<bool> Input)
        {
            long ret = 0;
            foreach (bool b in Input)
	        {   
                ret = ret << 1;
                if (b)
                {
                    ret += 1;
                }
            }
            return ret;
        }
        public static List<bool> ToList(long Input, int length)
        {
            List<bool> ret = new List<bool>();
            for (int i = 0; i < length; i++)
            {
                if (Input % 2 == 0)
                {
                    ret.Add(false);
                }
                else
                {
                    ret.Add(true);
                }
                Input = Input >> 1;
            }
            ret.Reverse();
            return ret;
        }
    }
}
