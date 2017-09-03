using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Allax.Cryptography
{
    class NextStepTask : Allax.ITask
    {
        CA OrigAutomata;
        CA NewAutomata;
        int CellIndex;
        public NextStepTask(CA OrigAutomata, CA NewAutomata, int CellIndex)
        {
            this.OrigAutomata = OrigAutomata;
            this.NewAutomata = NewAutomata;
            this.CellIndex = CellIndex;
        }
        public void Exec()
        {
            NewAutomata[CellIndex].Value = OrigAutomata.GetNextStepCellValue(CellIndex);
        }
    }
    class NextStepTasker : Allax.ITasker
    {
        public int CellCount;
        private int CurCell = 0;
        public CA OrigAutomata;
        public CA NewAutomata;
        public void AddAllBruteForceTasks()
        {
            throw new NotImplementedException();
        }

        public void AddTask(Allax.Task T)
        {
            throw new NotImplementedException();
        }

        public List<Allax.ITask> DequeueTasks(int Count)
        {
            var ret = new List<Allax.ITask>();
            for (int i = 0; CurCell < CellCount; i++, CurCell++)
            {
                ret.Add(new NextStepTask(OrigAutomata, NewAutomata, CurCell));
            }
            return ret;
        }

        public ulong GetTasksCount()
        {
            throw new NotImplementedException();
        }

        public void Init(Allax.TaskerParams Params)
        {
            throw new NotImplementedException();
        }

        public void InitSolvers(Dictionary<Allax.AvailableSolverTypes, Allax.Solver> Solvers = null)
        {
            throw new NotImplementedException();
        }

        public bool IsFinished()
        {
            return !(CurCell < CellCount);
        }

        public void ProcessRules(List<Allax.Rule> Rules)
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            CurCell = 0;
        }
    }
}
