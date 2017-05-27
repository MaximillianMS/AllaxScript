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
        static void KLayer(SPNetWayBlock kblock)
        {
            kblock.active_outputs = kblock.active_inputs;
        }
        static void SLayer(SPNetWay Way, ISPNet Net, int LIndex, ref long MIN, ref long CurrentCor)
        {
            throw new NotImplementedException();
            for (int SIndex = 0; SIndex < Way.layers[LIndex].blocks.Count; SIndex++)
            {
                SPNetWayBlock WayBlock = Way.layers[LIndex].blocks[SIndex];
                IBlock NetBlock = Net.GetLayers()[LIndex].GetBlock((byte)SIndex);
                var States = NetBlock.ExtractStates(MIN, CurrentCor, WayConverter.ToByte(WayBlock.active_inputs));
                foreach (var State in States)
                {
                    var Outputs = WayConverter.ToList(State._outputs, WayBlock.active_outputs.Count);
                    WayBlock.active_outputs = Outputs;
                    CurrentCor *= State._cor;
                    //Solver(Net, Way, ref MIN, CurrentCor);
                }
            }
        }
        static void PLayer()
        {
            throw new NotImplementedException();
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
                    if (lastNotEmptyLayerIndex % 3 == 0)
                    {
                        KLayer(Way.layers[lastNotEmptyLayerIndex].blocks[0]);
                        lastNotEmptyLayerIndex++;
                    }
                    #endregion
                    #region S-layer
                    if (lastNotEmptyLayerIndex % 3 == 1)
                    {

                        SLayer(Way, Net, lastNotEmptyLayerIndex, ref MIN, ref CurrentCor);
                        lastNotEmptyLayerIndex++;
                    }
                    #endregion
                    #region P-layer
                    if (lastNotEmptyLayerIndex % 3 == 2)
                    {
                        lastNotEmptyLayerIndex++;
                    }
                    #endregion
                }
            }
            #endregion
            #region LastRound
            #endregion
        }
    }
}
