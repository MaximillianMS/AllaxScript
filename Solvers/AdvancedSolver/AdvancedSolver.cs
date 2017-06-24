using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Allax
{
    class FirstSolutionSolver : BaseSolver
    {
        private bool FirstTimeStarted = true;
        public bool GetFirstTime()
        {
            lock (syncRoot)
            {
                return FirstTimeStarted;
            }
        }
        private void SetFirstTime(bool Value)
        {
            lock(syncRoot)
            {
                FirstTimeStarted = Value;
            }
        }
        private static readonly object syncRoot = new object();
        public override void Solve(SolverParams SolParams)
        {
            if (GetFirstTime())
                base.Solve(SolParams);
        }
        protected override void FinishSolving(SolverParams SolParams)
        {
            lock (syncRoot)
            {
                if (GetFirstTime())
                {
                    base.FinishSolving(SolParams);
                    SetFirstTime(false);
                }
            }
        }
    }
    class AdvancedSolver:BaseSolver
    {
        private bool FirstTimeStarted=true;
        private static readonly object syncRoot = new object();
        public override void Solve(SolverParams SolParams)
        {
            if(FirstTimeStarted)
            {
                lock(syncRoot)
                {
                    if(FirstTimeStarted)
                    {
                        var TestSolParams = SolParams;
                        TestSolParams.MaxActiveBlocksOnLayer = 1;
                        var It = new InputsIterator(SolParams.Way.layers[1].blocks.Count, SolParams.Way.layers[1].blocks[0].BlockSize, 1);
                        for (int SolvesCount = 0; SolvesCount < 3 && !It.IsFinished();)
                        {
                            var ws = WayConverter.ToWay(SolParams.Engine.GetSPNetInstance(), It.NextState());
                            TestSolParams.Way = ws;
                            var FS = new FirstSolutionSolver();
                            FS.Solve(TestSolParams);
                            if (!FS.GetFirstTime())
                                SolvesCount++;
                        }
                        FirstTimeStarted = false;
                    }
                }
            }
            base.Solve(SolParams);
        }
        protected override int GetStatesCount(List<BlockState> States, SolverParams SolParams)
        {
            int ret = 1;
            if (States.Count == 0 || States.Count == 1)
            {
                ret = States.Count;
            }
            else
            {
                if (SolParams.lastNotEmptyLayerIndex > SolParams.Way.layers.Count - 6)
                    ret = GetMaxStates(States);
                else
                {
                    ret = GetMaxStates(States);
                    if (ret == States.Count)
                        return ret;
                    var LIndex = SolParams.lastNotEmptyLayerIndex;
                    var Round = LIndex / 3;
                    var Rounds = SolParams.Way.layers.Count;
                    var Modificator = ((double)(Rounds - 1 - Round)) / (Rounds - 1);
                    var MaxMValue = Math.Abs(States[0].MatrixValue);
                    var HalfMaxMValue = MaxMValue >> 1;
                    var QuaterMaxMValue = HalfMaxMValue >> 1;
                    var HalfQuaterMaxMValue = QuaterMaxMValue >> 1;
                    var Limit = (uint)(HalfMaxMValue + QuaterMaxMValue + HalfQuaterMaxMValue+ HalfQuaterMaxMValue * Modificator);
                    var HalfCount = (uint)States.Count >> 1;
                    for (; ret < HalfCount; ret++)
                    {
                        if (Math.Abs(States[ret].MatrixValue) < Limit)
                            break;
                    }
                    ret++;
                }
            }
            return ret;
        }
    }
}
