using System;
using System.Windows;

using Launcher.Properties;

namespace Launcher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private TracingViewModel _traceViewModel;
        private AnalyzerViewModel _analyzeViewModel;

        protected override void OnStartup(StartupEventArgs e)
        {
            //base.OnStartup(e);

            var outputDirectory = Environment.CurrentDirectory;

            var wnd = new MainWindow();
            _traceViewModel = new TracingViewModel();
            _analyzeViewModel = new AnalyzerViewModel();
            _traceViewModel.Target = Settings.Default.LastTarget;
            _traceViewModel.OutputDirectory = outputDirectory;
            _analyzeViewModel.WorkingDirectory = outputDirectory;
            _traceViewModel.TraceSourceChanged += _analyzeViewModel.RefreshAvailableTraces;
            _analyzeViewModel.RefreshAvailableTraces(null, new TracesArg { Path = outputDirectory });

            wnd._trace.DataContext = _traceViewModel;
            wnd._analyze.DataContext = _analyzeViewModel;
            wnd.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _traceViewModel.TraceSourceChanged -= _analyzeViewModel.RefreshAvailableTraces;

            Settings.Default.LastTarget = _traceViewModel.Target;
            Settings.Default.Save();

            base.OnExit(e);
        }
    }
}