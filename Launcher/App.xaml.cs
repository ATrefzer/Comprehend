using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

using Launcher.Execution;
using Launcher.Profiler;
using Launcher.Properties;

namespace Launcher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private TracingViewModel _traceViewModel;
        private CallGraphTabViewModel _callGraphTabViewModel;
        private SequenceTabViewModel _sequenceTabViewModel;

        protected override void OnStartup(StartupEventArgs e)
        {
            //base.OnStartup(e);

            var outputDirectory = Environment.CurrentDirectory;

            var wnd = new MainWindow();

            var service = new BackgroundExecutionService(wnd);

            _traceViewModel = new TracingViewModel();
            _sequenceTabViewModel = new SequenceTabViewModel(service);
            _callGraphTabViewModel = new CallGraphTabViewModel(service);
            _traceViewModel.Target = Settings.Default.LastTarget;
            _traceViewModel.OutputDirectory = outputDirectory;
            _callGraphTabViewModel.WorkingDirectory = outputDirectory;
            _traceViewModel.AvailableTracesChanged += TraceViewModelOnAvailableTracesChanged;

            wnd._trace.DataContext = _traceViewModel;
            wnd._callgraphTab.DataContext = _callGraphTabViewModel;
            wnd._sequenceTab.DataContext = _sequenceTabViewModel;

            // Load initially available traces from the output directory
            UpdateAvailableTraces();

            Current.MainWindow = wnd;
            wnd.Show();
        }

        protected void UpdateAvailableTraces()
        {
            var workingDirectory = _traceViewModel.OutputDirectory;

            var availableProfiles = new List<Profile>();

            if (Directory.Exists(workingDirectory))
            {
                var files = Directory.EnumerateFiles(workingDirectory, "*.index");
                foreach (var file in files)
                {
                    
                    var fi = new FileInfo(file);
                    var baseName = fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length);
                    var trace = new Profile(workingDirectory, baseName);
                    if (File.Exists(trace.EventFile))
                    {
                        availableProfiles.Add(trace);
                    }
                }
            }

            // Update all view models
            _callGraphTabViewModel.RefreshAvailableProfiles(workingDirectory, availableProfiles);
            _sequenceTabViewModel.RefreshAvailableProfiles(workingDirectory, availableProfiles);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _traceViewModel.AvailableTracesChanged -= TraceViewModelOnAvailableTracesChanged;

            Settings.Default.LastTarget = _traceViewModel.Target;
            Settings.Default.Save();

            base.OnExit(e);
        }

        private void TraceViewModelOnAvailableTracesChanged(object sender, TracesArg e)
        {
            // Propagate to all other view models. Read files only once
            UpdateAvailableTraces();
        }
    }
}