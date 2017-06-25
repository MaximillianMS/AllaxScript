using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Allax
{
    public interface ITasker
    {
        List<ITask> DequeueTasks(int Count);
        ulong GetTasksCount();
        void AddTask(Task T);
        void Init(TaskerParams Params);
        bool IsFinished();
        void InitSolvers(Dictionary<AvailableSolverTypes, Solver> Solvers = null);
        void ProcessRules(List<Rule> Rules);
        void AddAllBruteForceTasks();
        void Reset();
    }
}
