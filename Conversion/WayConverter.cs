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
        public static SPNetWay ToEmptyWay(ISPNet Net)
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
            throw new NotImplementedException();
        }
        public static byte ToByte(List<bool> Input)
        {
            throw new NotImplementedException();
            byte ret = 0;

            return ret;
        }
        public static List<bool> ToList(int Input, int length)
        {
            throw new NotImplementedException();
            var ret = new List<bool>(length);

            return ret;
        }
    }
}
