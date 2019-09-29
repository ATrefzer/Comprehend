using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using GraphLibrary.Dgml;

namespace Launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Button_Click_x64(object sender, RoutedEventArgs e)
        {
            Environment.SetEnvironmentVariable("COR_PROFILER", "{7E981B79-6303-483F-A372-8169B1073A0F}");
            Environment.SetEnvironmentVariable("COR_ENABLE_PROFILING", "1");

            var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var profilerDll = Path.Combine(directory, "MiniProfiler_x64.dll");

            // The COM object is not registered. Instead it is sufficient to pass the file path to the profiler dll.
            Environment.SetEnvironmentVariable("COR_PROFILER_PATH", profilerDll);

            // Start child process and inherit environment variables
            IsEnabled = false;
            await Task.Run(() =>
                           {
                               var process = Process.Start(Path.Combine(directory, "HelloWorld_x64"));
                               process?.WaitForExit();
                           });
            IsEnabled = true;
        }    

        private async void Button_Click_x86(object sender, RoutedEventArgs e)
        {
            Environment.SetEnvironmentVariable("COR_PROFILER", "{7E981B79-6303-483F-A372-8169B1073A0F}");
            Environment.SetEnvironmentVariable("COR_ENABLE_PROFILING", "1");

            var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var profilerDll = Path.Combine(directory, "MiniProfiler_x86.dll");

            // The COM object is not registered. Instead it is sufficient to pass the file path to the profiler dll.
            Environment.SetEnvironmentVariable("COR_PROFILER_PATH", profilerDll);

            // Start child process and inherit environment variables

            var path = Path.Combine(directory, "HelloWorld_x86");
            

            //IsEnabled = false;
            //await Task.Run(() =>
            //               {
            //                   var process = Process.Start(path);
            //                   process?.WaitForExit();
            //               });
            //IsEnabled = true;

            ProfileParser parser = new ProfileParser();

            var eventStream = parser.Parse();
            
            // TODO Wohin??
            var model = parser.CreateInvokationModel(eventStream);


            var file = "d:\\outgraph.dgml";
            DgmlFileBuilder builder = new DgmlFileBuilder();

            foreach (var func in model.AllFunctions)
            {
                foreach (var call in func.Children)
                {
                    builder.AddEdge(func.Name, call.Name);
                }
            }

            builder.WriteOutput(file);

        }

        private void Button_Click_RunTests(object sender, RoutedEventArgs e)
        {
            ProfilerExports.RunTests();
        }
    }
}