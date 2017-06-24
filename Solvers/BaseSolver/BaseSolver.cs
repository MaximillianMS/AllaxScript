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
            kblock.Outputs = kblock.Inputs;
            SolParams.Way.layers[LIndex].blocks[0] = kblock;
        }
        protected virtual bool SLayer(SolverParams SolParams)
        {
            bool ret = true; //just checking mode
            if(SolParams.BIndex == -1) // check MaxActiveBlocks condition
            {
                SolParams.BIndex = 0;
                if (!CheckActiveBlocksCondition(SolParams))
                {
                    return false;
                }
            }
            var Blocks = SolParams.Way.layers[SolParams.lastNotEmptyLayerIndex].blocks;
            for (; SolParams.BIndex <Blocks.Count; SolParams.BIndex++)
            {
                var WayBlock = Blocks[SolParams.BIndex];
                if (WayBlock.Inputs == 0)
                {
                    continue;
                }
                if (WayBlock.Outputs!=0)
                {
                    continue; //already solved block
                }
                ret = false; // Producing new branches mode.
                var NetBlock = SolParams.Engine.GetSPNetInstance().GetLayers()[SolParams.lastNotEmptyLayerIndex].GetBlocks()[SolParams.BIndex];
                var Params = new BlockStateExtrParams(WayBlock.Inputs, SolParams.Engine.GetMultiThreadPrevalence(), SolParams.P, SolParams.Type, true);
                var States = NetBlock.ExtractStates(Params);
                DepthFirstSearch(States, SolParams);
                break;
            }
            return ret;
        }
        protected virtual int GetMaxStates(List<BlockState> States)
        {
            var ret = 0;
            var max = Math.Abs(States[0].MatrixValue);
            for (int i=0 ; i < States.Count; i++)
            {
                if (Math.Abs(States[ret].MatrixValue) < max)
                {
                    break;
                }
                else
                {
                    ret++;
                }
            }
            return ret;
        }
        protected virtual int GetStatesCount(List<BlockState> States, SolverParams SolParams)
        {
            if (SolParams.lastNotEmptyLayerIndex > SolParams.Way.layers.Count - 6 && States.Count != 0)
            {
                return GetMaxStates(States);
            }
            else
                return States.Count;
        }
        protected virtual void DepthFirstSearch(List<BlockState> States, SolverParams SolParams)
        {
            var Count = GetStatesCount(States, SolParams);
            for (int i = 0; i < Count; i++)
            {
                var State = States[i];
                var NewWay = WayConverter.CloneWay(SolParams.Way);
                var NewBLock = NewWay.layers[SolParams.lastNotEmptyLayerIndex].blocks[SolParams.BIndex];
                NewBLock.Outputs = State.outputs;
                NewWay.layers[SolParams.lastNotEmptyLayerIndex].blocks[SolParams.BIndex] = NewBLock;
                var NewSolParams = SolParams;
                NewSolParams.P *= State.MatrixValue;
                NewSolParams.Way = NewWay;
                NewSolParams.BIndex++;
                Solve(NewSolParams);
            }

        }
        bool CheckActiveBlocksCondition(SolverParams SolParams)
        {
            int ActiveBlocksCount = 0;
            SolParams.BIndex = 0;
            for (; SolParams.BIndex < SolParams.Way.layers[SolParams.lastNotEmptyLayerIndex].blocks.Count; SolParams.BIndex++)
            {
                var WayBlock = SolParams.Way.layers[SolParams.lastNotEmptyLayerIndex].blocks[SolParams.BIndex];
                if (WayBlock.Inputs!=0)
                {
                    ActiveBlocksCount++;
                    continue;
                }
            }
            return ActiveBlocksCount <= SolParams.MaxActiveBlocksOnLayer;
        }
        protected virtual void PLayer(SolverParams SolParams)
        {
            var LIndex = SolParams.lastNotEmptyLayerIndex;
            var Block = SolParams.Way.layers[LIndex].blocks[0];
            var Params = new BlockStateExtrParams(Block.Inputs,
                                                                new Prevalence(), new Prevalence(), SolParams.Type);
            var States = SolParams.Engine.GetSPNetInstance().GetLayers()[LIndex].GetBlocks()[0].ExtractStates(Params);
            if (States.Count == 1)
            {
                Block.Outputs = WayConverter.ToLong(States[0].CustomOutput);
                SolParams.Way.layers[LIndex].blocks[0] = Block;
            }
            else
            {
                Logger.UltraLogger.Instance.AddToLog("BaseSolver: Cant extract state from P-layer", Logger.MsgType.Error);
                throw new NotImplementedException();
            }
        }
        protected virtual bool FullRound(ref SolverParams SolParams)
        {
            #region K-layer
            if (SolParams.Way.layers[SolParams.lastNotEmptyLayerIndex].type == LayerType.KLayer)
            {
                KLayer(SolParams);
                WayConverter.CopyOutToIn(SolParams.Way, SolParams.lastNotEmptyLayerIndex, SolParams.lastNotEmptyLayerIndex + 1);
                SolParams.lastNotEmptyLayerIndex++;
                SolParams.BIndex = -1; //reset after previous S-layer
            }
            #endregion
            #region S-layer
            if (SolParams.Way.layers[SolParams.lastNotEmptyLayerIndex].type == LayerType.SLayer)
            {
                if (!(SLayer(SolParams)))
                {
                    return false;
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
            return true;
        }
        protected virtual void FinishSolving(SolverParams SolParams)
        {
            if (SolParams.P > SolParams.Engine.GetMultiThreadPrevalence())
            {
                SolParams.Engine.SetMultiThreadPrevalence(SolParams.P);
            }
            SolParams.Engine.GetSettings().AddSolutionFunc(new Solution(SolParams.P, SolParams.Way));

        }
        public virtual void Solve(SolverParams SolParams)
        {
            var layersCount = SolParams.Way.layers.Count();
            var roundsCount = layersCount / 3;
            #region FindLastNotEmptyLayer
            if (SolParams.lastNotEmptyLayerIndex == -1)
            {
                SolParams.lastNotEmptyLayerIndex = WayConverter.SearchLastNotEmptyLayer(SolParams.Way);
                if (SolParams.lastNotEmptyLayerIndex == -1)
                    return;
            }
            #endregion
            #region FullRounds
            if ((SolParams.lastNotEmptyLayerIndex >= 0) && (SolParams.lastNotEmptyLayerIndex / 3 < roundsCount - 1))
            {
                for (int i = SolParams.lastNotEmptyLayerIndex / 3; i < roundsCount - 1; i++)
                {
                    if (!FullRound(ref SolParams))
                        return;
                }
            }
            #endregion
            #region LastRound
            //No need to process LastRound, because LastRound must be reversed.
            #endregion
            FinishSolving(SolParams);
        }
    }
}
