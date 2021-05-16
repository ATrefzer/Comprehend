using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Launcher.Profiler
{
    internal class Process
    {
        public static Task StartAsync(string target, string profilerDirectory, string outputDirectory)
        {
            // Setup environment variables passed to the profiled process
            Environment.SetEnvironmentVariable("MINI_PROFILER_OUT_DIR", outputDirectory);
            Environment.SetEnvironmentVariable("COR_PROFILER", "{7E981B79-6303-483F-A372-8169B1073A0F}");
            Environment.SetEnvironmentVariable("COR_ENABLE_PROFILING", "1");

            // Select correct profiler dll.
            string profilerDll;
            if (Is64Bit(target))
            {
                profilerDll = Path.Combine(profilerDirectory, "MiniProfiler_x64.dll");
            }
            else
            {
                profilerDll = Path.Combine(profilerDirectory, "MiniProfiler_x86.dll");
            }

            // The COM object is not registered. Instead it is sufficient to pass the file path to the profiler dll.
            Environment.SetEnvironmentVariable("COR_PROFILER_PATH", profilerDll);

            // Start child process and inherit environment variables

            return Task.Run(() =>
                            {
                                var process = System.Diagnostics.Process.Start(target);
                                process?.WaitForExit();
                            });
        }


        private static bool Is64Bit(string path)
        {
            // Launcher is compiled x64 always.

            var assembly = Assembly.LoadFile(Path.GetFullPath(path));
            var manifestModule = assembly.ManifestModule;
            PortableExecutableKinds peKind;
            manifestModule.GetPEKind(out peKind, out var machine);

            return machine == ImageFileMachine.AMD64;
        }
    }
}