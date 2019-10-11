using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

using GraphLibrary;
using GraphLibrary.Dgml;
using GraphLibrary.Msagl;

using Launcher.Execution;
using Launcher.Models;
using Launcher.Profiler;

using Prism.Commands;

using Process = System.Diagnostics.Process;

namespace Launcher
{
    internal class AnalyzerViewModel : INotifyPropertyChanged
    {
        private readonly BackgroundExecutionService _backgroundService;
        private Profile _selectedProfile;


        public AnalyzerViewModel(BackgroundExecutionService backgroundService)
        {
            _backgroundService = backgroundService;
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
                    OnPropertyChanged(nameof(IsProfileSelected));

                    //CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public ObservableCollection<Profile> AvailableProfiles { get; } = new ObservableCollection<Profile>();

        public bool IsProfileSelected => SelectedProfile != null;

        public void RefreshAvailableProfiles(object sender, TracesArg args)
        {
            WorkingDirectory = args.Path;
            RefreshAvailableProfiles();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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


        private CallGraphModel ProcessProfile(IProgress progress, Profile profile)
        {
            //var filter = Filter.Default();
            var filter = Filter.FromFile(GetFilterFilePath());

            var parser = new ProfileParser(progress);

            // Add filter here only for performance.
            var eventStream = parser.Parse(profile.IndexFile, profile.EventFile, filter);

            var model = CallGraphModel.FromEventStream(eventStream);

            return model;
        }

        string GetOutputDgmlFile(Profile profile)
        {
            return Path.Combine(WorkingDirectory, SelectedProfile + ".graph.dgml");
        }

        private async Task ExecuteGenerateFilteredGraphAsync()
        {
            var profile = SelectedProfile;
            if (profile == null)
            {
                Debug.Assert(false);
                return;
            }

            CallGraphModel model = null;
            try
            {
                await _backgroundService.RunWithProgress(progress => model = ProcessProfile(progress, profile));

                var exporter = new CallGraphExporter();

                if (model != null)
                {
                    // Export to dgml
                    var dgml = new DgmlFileBuilder();
                    exporter.Export(model, dgml);
                    dgml.WriteOutput(GetOutputDgmlFile(profile));

                    // Show graph in window
                    var msagl = new MsaglGraphWrapper();
                    exporter.Export(model, msagl);
                    msagl.ShowResult();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Reading profile file failed!");
            }
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