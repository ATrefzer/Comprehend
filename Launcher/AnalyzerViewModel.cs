using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using Launcher.Models;

using Prism.Commands;

namespace Launcher
{
    internal class AnalyzerViewModel : INotifyPropertyChanged
    {
        private Profile _selectedProfile;


        public AnalyzerViewModel()
        {
            GenerateFilteredGraphCommand = new DelegateCommand(async () => await ExecuteGenerateFilteredGraphAsync());

            //GenerateFilteredGraphCommand = new DelegateCommand(ExecuteGenerateFilteredGraph, IsTraceSelected);
            EditFilterCommand = new DelegateCommand(ExecuteEditFilter);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public string WorkingDirectory { get; set; }
        public ICommand GenerateFilteredGraphCommand { get; }
        public ICommand EditFilterCommand { get; }

        public Profile SelectedProfile
        {
            get => _selectedProfile;
            set
            {
                if (_selectedProfile != value)
                {
                    _selectedProfile = value;
                    OnPropertyChanged();

                    //CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public ObservableCollection<Profile> AvailableProfiles { get; } = new ObservableCollection<Profile>();

        public void RefreshAvailableProfiles(object sender, TracesArg args)
        {
            WorkingDirectory = args.Path;
            RefreshAvailableProfiles();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool IsTraceSelected()
        {
            return SelectedProfile != null;
        }

        private string GetFilterFilePath()
        {
            // Assume filter file
            var assembly = Assembly.GetCallingAssembly().Location;
            var fi = new FileInfo(assembly);
            var launcherDir = fi.DirectoryName ?? ".\\";
            var filterDef = Path.Combine(launcherDir, "filters.txt");

            return filterDef;
        }

        private void ExecuteEditFilter()
        {
            var filterDef = GetFilterFilePath();
            if (!File.Exists(filterDef))
            {
                using (File.CreateText(filterDef))
                {
                    // Just create
                }
            }

            Process.Start(filterDef);
        }

        private async Task ExecuteGenerateFilteredGraphAsync()
        {
            var selection = SelectedProfile;
            if (selection == null)
            {
                Debug.Assert(false);
                return;
            }

            var filter = Filter.FromFile(GetFilterFilePath());

            var progressWindow = new ProgressWindow();
            progressWindow.Owner = Application.Current.MainWindow;
            progressWindow._progressBar.Value = 0;
            progressWindow.Show();

            await Task.Run(() =>
                     {

                         //var filter = Filter.Default();
                         var parser = new ProfileParser(progressWindow);

                         // Add filter here only for performance.
                         var eventStream = parser.Parse(selection.IndexFile, selection.EventFile, filter);

                         var model = CallGraphModel.FromEventStream(eventStream);

                         // Write output file
                         var exporter = new CallGraphExporter();
                         var file = Path.Combine(WorkingDirectory, SelectedProfile + ".graph.dgml");
                         exporter.Export(model, file);

                     });

            // Dispose
            progressWindow._progressBar.Value = 100;
            progressWindow.Hide();

        }

        private void RefreshAvailableProfiles()
        {
            AvailableProfiles.Clear();

            if (!Directory.Exists(WorkingDirectory))
            {
                return;
            }

            var files = Directory.EnumerateFiles(WorkingDirectory, "*.index");
            foreach (var file in files)
            {
                var fi = new FileInfo(file);
                var baseName = fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length);
                var trace = new Profile(WorkingDirectory, baseName);
                AvailableProfiles.Add(trace);
            }
        }
    }
}