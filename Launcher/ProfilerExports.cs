﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
   
    static class ProfilerExports
    {
        [DllImport("MiniProfiler_x86.dll")]
        public static extern void RunTests();
    }
}
