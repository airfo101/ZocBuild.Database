﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database.Build
{
    internal interface IScriptExecutor
    {
        Task ExecuteAsync(ScriptFile script, BuildItem.BuildActionType action);
    }
}
