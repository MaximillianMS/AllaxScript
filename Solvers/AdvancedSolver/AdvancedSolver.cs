using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Allax
{
    class AdvancedSolver:BaseSolver
    {
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
                    ret = 1;
                else
                {
                    var LIndex = SolParams.lastNotEmptyLayerIndex;
                    var Round = LIndex / 3;
                    var Rounds = SolParams.Way.layers.Count;
                    var Modificator = ((double)(Rounds - 1 - Round)) / (Rounds - 1);
                    var Max = (uint)1 << States[0].BlockSize;
                    var HalfMax = Max >> 1;
                    var QuaterMax = HalfMax >> 1;
                    var HalfQuaterMax = QuaterMax >> 1;
                    var Limit = (uint)(HalfQuaterMax + HalfQuaterMax * Modificator);
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
