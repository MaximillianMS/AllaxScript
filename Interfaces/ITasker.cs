﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Allax
{
    public interface ITasker
    {
        List<Task> GetTasks(int Count);
        void AddTask(Task T);
        void Init(TaskerParams Params);
        bool IsFinished();
    }
}
