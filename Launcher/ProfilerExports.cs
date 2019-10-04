using System.Runtime.InteropServices;

namespace Launcher
{
    internal static class ProfilerExports
    {
        [DllImport("MiniProfiler_x86.dll")]
        public static extern void RunTests();
    }
}