using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using GraphFormats.PlantUml;

using Launcher.Execution;
using Launcher.Models;
using Launcher.Profiler;

using Prism.Commands;

using Process = System.Diagnostics.Process;

namespace Launcher
{
    internal class SequenceViewModel : INotifyPropertyChanged
    {
        private readonly BackgroundExecutionService _backgroundService;
        private Profile _selectedProfile;


        public SequenceViewModel(BackgroundExecutionService backgroundService)
        {
            _backgroundService = backgroundService;
            GenerateSequenceDiagramCommand = new DelegateCommand(async () => await ExecuteGenerateSequenceDiagramAsync());

            //GenerateSequenceDiagramCommand = new DelegateCommand(ExecuteGenerateFilteredGraph, IsTraceSelected);
            EditFilterCommand = new DelegateCommand(ExecuteEditFilter);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public string WorkingDirectory { get; set; }
        public ICommand GenerateSequenceDiagramCommand { get; }
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
                }
            }
        }

        public ObservableCollection<Profile> AvailableProfiles { get; } = new ObservableCollection<Profile>();

        public bool IsProfileSelected => SelectedProfile != null;

        public void RefreshAvailableProfiles(string workingDirectory, List<Profile> availableProfiles)
        {
            WorkingDirectory = workingDirectory;
            AvailableProfiles.Clear();
            foreach (var profile in availableProfiles)
            {
                AvailableProfiles.Add(profile);
            }

            SelectedProfile = null;
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


        private SequenceModel ProcessProfile(IProgress progress, Profile profile)
        {
            //var filter = Filter.Default();
            var filter = Filter.FromFile(GetFilterFilePath());

            if (filter.GetEntryFunctions().Count != 1)
            {
                throw new Exception("To generate a sequence diagram you need one entry function!");
            }

            var parser = new ProfileParser(progress);

            // Add filter here only for performance.
            var eventStream = parser.Parse(profile.IndexFile, profile.EventFile, filter);

            var model = SequenceModel.FromEventStream(eventStream);

            return model;
        }

        private string GetOutputPlantumlFile(Profile profile)
        {
            return Path.Combine(WorkingDirectory, SelectedProfile + ".graph.plantuml");
        }

        private async Task ExecuteGenerateSequenceDiagramAsync()
        {
            var profile = SelectedProfile;
            if (profile == null)
            {
                Debug.Assert(false);
                return;
            }

            SequenceModel model = null;
            try
            {
                await _backgroundService.RunWithProgress(progress => model = ProcessProfile(progress, profile));

                var exporter = new SequenceModelExporter();

                if (model != null)
                {
                    var builder = new PlantUmlBuilder();
                    builder.AddCategory("indirect", "color", "#0000FF");
                    exporter.Export(model, builder);
                    builder.WriteOutput(GetOutputPlantumlFile(profile));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Reading profile file failed!");
            }
        }
    }
}