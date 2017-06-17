using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Allax
{
    public static class WayConverter
    {
        public static List<byte> MatrixToList(List<List<bool>> FuncMatrix)
        {
            var ret = new List<byte>(FuncMatrix.Count);
            for(int i=0;i<FuncMatrix.Count;i++)
            {
                ret.Add((byte)ToLong(FuncMatrix[i]));
            }
            return ret;
        }
        public static List<List<bool>> ListToMatrix(List<byte> arg, int VarCount)
        {
            var FuncMatrix = new  List<List<bool>>(arg.Count);
            FuncMatrix.AddRange(Enumerable.Range(0, arg.Count).Select(i => new List<bool>().ToList()));
            for (int i =0; i < arg.Count; i++)
            {
                FuncMatrix[i] = ToList(arg[i], VarCount);
            }
            return FuncMatrix;
        }
        public static string MatrixToString(List<List<bool>> funcMatrix)
        {
            var ret = "";
            for (int i = 0; i < funcMatrix.Count; i++)
            {
                ret += ToLong(funcMatrix[i]).ToString();
            }
            return ret;
        }
        public static int SearchLastNotEmptyLayer(SPNetWay Way)
        {
            int index = -1;
            for (int i = 0; i < Way.layers.Count; i++)
            {
                if (Way.layers[i].blocks.All(x => x.Inputs == 0))
                {
                    index = i - 1;
                    break;
                }
            }
            return index;
        }
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
                    tmp.blocks = new List<SPNetWayBlock>();
                    if (tmp.type != LayerType.SLayer)
                    {
                        var tmp_block = new SPNetWayBlock();
                        tmp_block.Inputs = 0;
                        tmp_block.Outputs = 0;
                        tmp_block.BlockSize = Settings.WordLength;
                        tmp.blocks.Add(tmp_block);
                    }
                    else
                    {
                        foreach (var j in Enumerable.Range(0, Settings.SBoxCount))
                        {
                            var tmp_sblock = new SPNetWayBlock();
                            tmp_sblock.Inputs = 0;
                            tmp_sblock.Outputs = 0;
                            tmp_sblock.BlockSize = Settings.SBoxSize;
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
                        tmp_layer.blocks = new List<SPNetWayBlock>(Way.layers[i].blocks.Count);
                        for (int j = 0; j < Way.layers[i].blocks.Count; j++)
                        {
                            var tmp_block = new SPNetWayBlock();
                            tmp_block.Inputs = (empty) ? 0 : Way.layers[i].blocks[j].Inputs;
                            tmp_block.Outputs = (empty) ? 0 : Way.layers[i].blocks[j].Outputs;
                            tmp_block.BlockSize = Way.layers[i].blocks[j].BlockSize;
                            tmp_layer.blocks.Add(tmp_block);
                        }
                    }
                    NewWay.layers.Add(tmp_layer);
                }
            }
            return NewWay;
        }
        public static SPNetWay ToWay(ISPNet Net, SolverInputs Input)
        {
            var Way = ToWay(Net);
            var Block = Way.layers[0].blocks[0];
            Block.Inputs = Input.Input;
            Way.layers[0].blocks[0] = Block;
            return Way;
        }
        public static void CopyOutToIn(SPNetWay Way, int SrcLIndex, int DestLIndex)
        {
            if (SrcLIndex < Way.layers.Count && DestLIndex < Way.layers.Count)
            {
                #region From S-layer to P-layer
                if (Way.layers[SrcLIndex].type == LayerType.SLayer && Way.layers[DestLIndex].type == LayerType.PLayer)
                {
                    var PBlock = Way.layers[DestLIndex].blocks[0];
                    PBlock.Inputs = 0;
                    var Offset = Way.layers[SrcLIndex].blocks[0].BlockSize;
                    var Count = Way.layers[SrcLIndex].blocks.Count;
                    for (int i = 0; i < Count; i++)
                    {
                        PBlock.Inputs |= Way.layers[SrcLIndex].blocks[i].Outputs << ((Count - 1 - i) * Offset);
                    }
                    Way.layers[DestLIndex].blocks[0] = PBlock;
                }
                #endregion
                #region From P-Layer to K-Layer
                if (Way.layers[SrcLIndex].type == LayerType.PLayer && Way.layers[DestLIndex].type == LayerType.KLayer)
                {
                    var block = Way.layers[DestLIndex].blocks[0];
                    block.Inputs = Way.layers[SrcLIndex].blocks[0].Outputs;
                    Way.layers[DestLIndex].blocks[0] = block;
                }
                #endregion
                #region From K-Layer to S-Layer
                if (Way.layers[SrcLIndex].type == LayerType.KLayer && Way.layers[DestLIndex].type == LayerType.SLayer)
                {
                    var Offset = Way.layers[DestLIndex].blocks[0].BlockSize;
                    var SBlocksCount = Way.layers[DestLIndex].blocks.Count;
                    for (int i = 0; i < SBlocksCount; i++)
                    {
                        var SBlock = Way.layers[DestLIndex].blocks[i];
                        SBlock.Inputs = Way.layers[SrcLIndex].blocks[0].Inputs;
                        SBlock.Inputs = (64 - Offset * (SBlocksCount - i + 1));
                        SBlock.Inputs = (((uint)Way.layers[SrcLIndex].blocks[0].Inputs) << (64 - Offset * (SBlocksCount - i))) >> (64 - Offset);
                        Way.layers[DestLIndex].blocks[i] = SBlock;
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
            List<bool> ret = new List<bool>(length);
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
