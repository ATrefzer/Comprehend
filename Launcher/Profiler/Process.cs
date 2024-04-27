using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Launcher.Profiler
{
    internal class Process
    {
        public static Task StartAsync(string target, string profilerDirectory, string outputDirectory, bool isX64)
        {
            // Select correct profiler dll.
            string profilerDll;
            if (isX64)
            {
                profilerDll = Path.Combine(profilerDirectory, "MiniProfiler_x64.dll");
            }
            else
            {
                profilerDll = Path.Combine(profilerDirectory, "MiniProfiler_x86.dll");
            }

            Debug.Assert(File.Exists(profilerDll));

            // .NET 4.xx
            // Setup environment variables passed to the profiled process
            Environment.SetEnvironmentVariable("MINI_PROFILER_OUT_DIR", outputDirectory);
            Environment.SetEnvironmentVariable("COR_PROFILER", "{7E981B79-6303-483F-A372-8169B1073A0F}");
            Environment.SetEnvironmentVariable("COR_ENABLE_PROFILING", "1");
            // The COM object is not registered. Instead, it is sufficient to pass the file path to the profiler dll.
            Environment.SetEnvironmentVariable("COR_PROFILER_PATH", profilerDll);

            // .NET6 and above
            Environment.SetEnvironmentVariable("CORECLR_PROFILER_PATH", profilerDll);
            Environment.SetEnvironmentVariable("CORECLR_PROFILER", "{7E981B79-6303-483F-A372-8169B1073A0F}");
            Environment.SetEnvironmentVariable("CORECLR_ENABLE_PROFILING", "1");
            Environment.SetEnvironmentVariable("DD_PROFILING_ENABLED", "1");


            // Start child process and inherit environment variables
            return Task.Run(() =>
                            {
                                var process = System.Diagnostics.Process.Start(target);
                                process?.WaitForExit();
                            });
        }
    }
}