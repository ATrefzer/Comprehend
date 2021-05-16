using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GraphFormats.Dgml;
using GraphFormats.Msagl;
using Launcher.Execution;
using Launcher.Models;
using Launcher.Profiler;
using Prism.Commands;
using Process = System.Diagnostics.Process;

namespace Launcher
{
    internal sealed class CallGraphTabViewModel : INotifyPropertyChanged
    {
        private readonly BackgroundExecutionService _backgroundService;

        private CallGraph _callGraph;
        private Dictionary<ulong, FunctionInfo> _idToFunctionInfo;
        private Profile _selectedProfile;


        public CallGraphTabViewModel(BackgroundExecutionService backgroundService)
        {
            _backgroundService = backgroundService;
            OpenMethodChooserCommand = new DelegateCommand(OpenFunctionPickerAsync);

            EditFilterCommand = new DelegateCommand(ExecuteEditFilter);
        }

        public string WorkingDirectory { get; set; }
        public ICommand OpenMethodChooserCommand { get; }
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

        /// <summary>
        /// Called from the function picker.
        /// </summary>
        public async Task ExecuteGenerate(FunctionPickerViewModel pickerViewModel)
        {
            var profile = SelectedProfile;
            if (profile == null)
            {
                Debug.Assert(false);
                return;
            }

            try
            {
                await _backgroundService.RunWithProgress(progress => _callGraph = ProcessProfile(progress, profile));

                var exporter = new CallGraphExport();

                if (_callGraph != null)
                {
                    if (_callGraph.AllFunctions.Count(func => func.IsBanned == false) > 100)
                    {
                        MessageBox.Show("There are more than 100 functions in your call graph. I'm not drawing it.");
                        return;
                    }


                    var included = pickerViewModel.GetIncludedFunctionIds();

                    // Export to dgml
                    var dgml = new DgmlFileBuilder();
                    exporter.Export(_callGraph, included, dgml);
                    dgml.WriteOutput(GetOutputDgmlFile(profile));

                    // Show graph in window
                    var msagl = new MsaglGraphBuilder();
                    exporter.Export(_callGraph, included, msagl);
                    msagl.ShowResult();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Reading profile file failed!");
            }
        }

        public HashSet<FunctionInfo> GetModelFunctions()
        {
            return new HashSet<FunctionInfo>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

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

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
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


        private CallGraph ProcessProfile(IProgress progress, Profile profile)
        {

            var parser = new ProfileParser(progress);

            // Add filter here only for performance.
            var eventStream = parser.ParseEventStream(profile.EventFile, _idToFunctionInfo);

            var callGraph = CallGraph.FromEventStream(eventStream);
            return callGraph;
        }

        private string GetOutputDgmlFile(Profile profile)
        {
            return Path.Combine(WorkingDirectory, profile + ".graph.dgml");
        }

        private void OpenFunctionPickerAsync()
        {
            var profile = SelectedProfile;
            Debug.Assert(profile != null);

            _callGraph = null;
            var preFilter = Filter.FromFile(GetFilterFilePath());
            var parser = new ProfileParser();
            _idToFunctionInfo = parser.ParseIndex(profile.IndexFile, preFilter);

            // All functions that are included according to the pre filter file
            // These functions can be hidden or made visible
            var preSelection = _idToFunctionInfo.Values.Where(info => !info.IsBanned).Select(info => new FunctionInfoViewModel(info));


            var pickerConfig = new FunctionPickerConfig
            {
                Instructions = Resources.Resources.InstructionCallGraph,
                DoFunc = ExecuteGenerate,
                HasStartFunction = false,
                ShowIncludeColumn = true,
                ExecuteButtonText = "Generate Image"
            };
            var setupWindow = new FunctionPickerView();
            var viewModel = new FunctionPickerViewModel(pickerConfig);
            viewModel.Initialize(preSelection);
            setupWindow.DataContext = viewModel;
            setupWindow.ShowDialog();
        }
    }
}