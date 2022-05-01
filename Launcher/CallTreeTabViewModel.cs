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
using Launcher.Execution;
using Launcher.Models;
using Launcher.Profiler;
using Prism.Commands;
using Process = System.Diagnostics.Process;

namespace Launcher
{
    internal class CallTreeTabViewModel : INotifyPropertyChanged
    {
        private readonly BackgroundExecutionService _backgroundService;

        private CallTree _callTrace;
        private Dictionary<ulong, FunctionInfo> _idToFunctionInfo;
        private Profile _selectedProfile;


        public CallTreeTabViewModel(BackgroundExecutionService backgroundService)
        {
            _backgroundService = backgroundService;
            LoadCommand = new DelegateCommand(async () => await ExecuteLoad());

            EditFilterCommand = new DelegateCommand(ExecuteEditFilter);
        }

        public string WorkingDirectory { get; set; }
        public ICommand LoadCommand { get; }
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

        public async Task ExecuteLoad()
        {
            var profile = SelectedProfile;
            if (profile == null)
            {
                Debug.Assert(false);
                return;
            }

            try
            {
                _callTrace = null;

                // Get start function. The included functions is determined later in the call tree explorer view.
                var startFunction = GetStartFunction(profile);
                if (startFunction == null)
                {
                    MessageBox.Show("No start function selected!");
                    return;
                }


                // Find traces in profile
                await _backgroundService.RunWithProgress(progress => _callTrace = LoadFromProfile(progress, profile, startFunction));
                if (!_callTrace.SequenceVariations.Any())
                {
                    MessageBox.Show("No sequences found for the start function!");
                    return;
                }


                var funcCall = _callTrace.SequenceVariations.First();

                // Add a dummy caller if we have more than one trace with the given start function
                if (_callTrace.SequenceVariations.Count > 1)
                {
                    var actor = FunctionCall.GetActor();
                    foreach (var sequence in _callTrace.SequenceVariations)
                    {
                        actor.Children.Add(sequence);
                    }

                    funcCall = actor;
                }


                // Open explorer view with all traces
                var vm = new CallTreeExplorerViewModel();
                vm.Roots.Add(new FunctionCallViewModel(funcCall));
                var view = new CallTreeExplorerView();
                view.ExportAction = Export;
                view.DataContext = vm;
                view.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Reading profile file failed!");
            }
        }

        private FunctionInfo GetStartFunction(Profile profile)
        {
            var preFilter = Filter.FromFile(GetFilterFilePath());
            var parser = new ProfileParser();
            _idToFunctionInfo = parser.ParseIndex(profile.IndexFile, preFilter);


            // All functions that are included according to the pre filter file
            // These functions can be hidden or made visible
            var preSelection = _idToFunctionInfo.Values.Where(info => !info.IsBanned).Select(info => new FunctionInfoViewModel(info));


            var wnd = new FunctionPickerView();

            Task CloseWindow(FunctionPickerViewModel pickerViewModel)
            {
                wnd.Close();
                return Task.CompletedTask;
            }

            var pickerConfig = new FunctionPickerConfig
            {
                Instructions = null,
                DoFunc = CloseWindow,
                HasStartFunction = true,
                ShowIncludeColumn = false,
                ExecuteButtonText = "OK"
            };

            var viewModel = new FunctionPickerViewModel(pickerConfig);
            viewModel.Initialize(preSelection);
            wnd.DataContext = viewModel;


            wnd.ShowDialog();

            return viewModel.StartFunction;
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


        private CallTree LoadFromProfile(IProgress progress, Profile profile, FunctionInfo startFunction)
        {
            if (_callTrace != null)
            {
                return null;
            }

            var parser = new ProfileParser(progress);

            // Read index
            var preFilter = Filter.FromFile(GetFilterFilePath());
            _idToFunctionInfo = parser.ParseIndex(profile.IndexFile, preFilter);


            // Add filter here only for performance.
            var eventStream = parser.ParseEventStream(profile.EventFile, _idToFunctionInfo);

            return CallTree.FromEventStream(eventStream, startFunction);
        }


        private void Export(FunctionCall root, bool simplify)
        {
            var fullPath = Assembly.GetExecutingAssembly().Location;
            if (_callTrace != null)
            {
                var exporter = new SequenceDiagramExport(_selectedProfile.GetName(), simplify);
                exporter.Export(GetOutputPlantumlFile(_selectedProfile), root);


                var exeDir = Path.GetDirectoryName(fullPath);

                var psi = new ProcessStartInfo();
                psi.FileName = "java.exe";
                psi.Arguments = "-jar " + Path.Combine(exeDir, "Dependencies", "plantuml.jar ") + GetOutputPlantumlFile(_selectedProfile) + " -tsvg";
                psi.CreateNoWindow = true;
                psi.RedirectStandardError = true;
                psi.RedirectStandardOutput = true;
                psi.UseShellExecute = false;

                var process = Process.Start(psi);
                if (process == null)
                {
                    return;
                }

                process.WaitForExit();

                if (process.ExitCode == -1)
                {
                    var error = process.StandardError.ReadToEnd();
                    throw new Exception(error);
                }

                var file = GetOutputSvgFile(_selectedProfile);
                var viewer = new SvgViewer();
                viewer.LoadImage(file);
                viewer.Show();
            }
        }


        private string GetOutputPlantumlFile(Profile profile)
        {
            return Path.Combine(profile.Directory, profile.GetName() + ".plantuml");
        }


        private string GetOutputSvgFile(Profile profile)
        {
            return Path.Combine(profile.Directory, profile.GetName() + ".svg");
        }
    }
}