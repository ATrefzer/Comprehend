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
    public partial class App
    {
        private ProfilerViewModel _profilerViewModel;
        private CallGraphTabViewModel _callGraphTabViewModel;
        private CallTreeTabViewModel _callTreeTabViewModel;

        protected override void OnStartup(StartupEventArgs e)
        {
            //base.OnStartup(e);

            var outputDirectory = Environment.CurrentDirectory;

            var wnd = new MainWindow();

            var service = new BackgroundExecutionService(wnd);

            _profilerViewModel = new ProfilerViewModel();
            _callGraphTabViewModel = new CallGraphTabViewModel(service);
            _callTreeTabViewModel = new CallTreeTabViewModel(service);

            _profilerViewModel.Target = Settings.Default.LastTarget;
            _profilerViewModel.OutputDirectory = outputDirectory;
            _callGraphTabViewModel.WorkingDirectory = outputDirectory;
            _profilerViewModel.AvailableTracesChanged += ProfilerViewModelOnAvailableProfilersChanged;

            wnd._trace.DataContext = _profilerViewModel;
            wnd._callGraphTab.DataContext = _callGraphTabViewModel;
            wnd._callTreeTab.DataContext = _callTreeTabViewModel;

            // Load initially available traces from the output directory
            UpdateAvailableTraces();

            Current.MainWindow = wnd;
            wnd.Show();
        }

        private void UpdateAvailableTraces()
        {
            var workingDirectory = _profilerViewModel.OutputDirectory;

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
            _callTreeTabViewModel.RefreshAvailableProfiles(workingDirectory, availableProfiles);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _profilerViewModel.AvailableTracesChanged -= ProfilerViewModelOnAvailableProfilersChanged;

            Settings.Default.LastTarget = _profilerViewModel.Target;
            Settings.Default.Save();

            base.OnExit(e);
        }

        private void ProfilerViewModelOnAvailableProfilersChanged(object sender, TracesArg e)
        {
            // Propagate to all other view models. Read files only once
            UpdateAvailableTraces();
        }
    }
}