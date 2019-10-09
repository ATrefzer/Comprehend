using System.Runtime.InteropServices;

namespace Launcher.Profiler
{
    internal static class ProfilerExports
    {
        [DllImport("MiniProfiler_x86.dll")]
        public static extern void RunTests();
    }
}