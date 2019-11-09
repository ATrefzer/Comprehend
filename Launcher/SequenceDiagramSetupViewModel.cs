using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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
    internal class SequenceDiagramSetupViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private readonly BackgroundExecutionService _backgroundService;
        private readonly string _workingDirectory;
        private Profile _profile;
        private Dictionary<ulong, FunctionInfo> _idToFunctionInfo;

        /// <summary>
        ///  Entry function(s) with all pre-filtered methods including those hidden functions necessary for indirect calls
        /// </summary>
        private SequenceModel _model;


        private FunctionInfo _startFunction;

        public SequenceDiagramSetupViewModel(BackgroundExecutionService backgroundService, string workingDirectory)
        {
            _backgroundService = backgroundService;
            _workingDirectory = workingDirectory;
            GenerateCommand = new DelegateCommand(Generate);
            SelectStartFunctionCommand = new DelegateCommand<FunctionInfoViewModel>(SelectStartFunction);
            IncludeCommand = new DelegateCommand<object>(Include);
            ExcludeCommand = new DelegateCommand<object>(Exclude);
            StartFunction = null;
        }

        public ICommand ExcludeCommand { get; set; }

        public ICommand IncludeCommand { get; set; }

        private void Exclude(object param )
        {
            var exclude = param as IList;
            if (exclude == null)
            {
                return;
            }
            foreach (FunctionInfoViewModel func in exclude)
            {
                func.Included = false;
            }
        }

        private void Include(object param)
        {
            var include = param as IList;
            if (include == null)
            {
                return;
            }

            foreach (FunctionInfoViewModel func in include)
            {
                func.Included = true;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public ICommand GenerateCommand { get; }


        public ObservableCollection<FunctionInfoViewModel> AllPreFilteredFunctions { get; set; }

        public FunctionInfo StartFunction
        {
            get => _startFunction;
            set
            {
                if (value == _startFunction)
                {
                    return;
                }

                _model = null;
                _startFunction = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanRender));
                OnErrorsChanged(new DataErrorsChangedEventArgs(nameof(StartFunction)));
            }
        }

        public bool CanRender => StartFunction != null;

        public ICommand SelectStartFunctionCommand { get; set; }

        public bool HasErrors => StartFunction == null;

        public void Initialize(Profile profile, Filter preFilter)
        {
            // Get all available function, pre-filtered
            _profile = profile;

            var parser = new ProfileParser();
            _idToFunctionInfo = parser.ParseIndex(profile.IndexFile, preFilter);

            var preFiltered = _idToFunctionInfo.Values.Where(info => !info.IsFiltered).Select(info => new FunctionInfoViewModel(info));
            AllPreFilteredFunctions = new ObservableCollection<FunctionInfoViewModel>(preFiltered);
        }

        public IEnumerable GetErrors(string propertyName)
        {
            if (propertyName == nameof(StartFunction))
            {
                if (StartFunction == null)
                {
                    return new List<string> { "You have to select a function to generate a sequence diagram!" };
                }
            }

            return Enumerable.Empty<string>();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnErrorsChanged(DataErrorsChangedEventArgs e)
        {
            ErrorsChanged?.Invoke(this, e);
        }

        private void SelectStartFunction(object startFunction)
        {
            if (startFunction != null)
            {
                var vm = startFunction as FunctionInfoViewModel;
                StartFunction = vm.Model;
            }
        }

        private string GetOutputPlantumlFile(Profile profile)
        {
            return Path.Combine(profile.Directory, profile.BaseFile.Replace(".", "_") + ".plantuml");
        }

        private string GetPlantUmlTitle(Profile profile)
        {
            return profile.BaseFile.Replace(".", "_");
        }

        private string GetOutputSvgFile(Profile profile)
        {
            return Path.Combine(profile.Directory, profile.BaseFile.Replace(".", "_") + ".svg");
        }

        private async void Generate()
        {
            try
            {
                await _backgroundService.RunWithProgress(progress => ProcessProfile(progress, _profile));

                // Model is available

                var exporter = new SequenceModelExporter();

                // TODO open new user interface
                var fullPath = Assembly.GetExecutingAssembly().Location;
                if (_model != null)
                {
                    var builder = new PlantUmlBuilder();
                    builder.Title = GetPlantUmlTitle(_profile);
                    exporter.Export(_model, builder);
                    builder.WriteOutput(GetOutputPlantumlFile(_profile));

                    var exeDir = Path.GetDirectoryName(fullPath);

                    var psi = new ProcessStartInfo();
                    psi.FileName = "java.exe";
                    psi.Arguments = "-jar " + Path.Combine(exeDir, "Dependencies", "plantuml.jar ") + GetOutputPlantumlFile(_profile) + " -tsvg";
                    psi.CreateNoWindow = true;
                    psi.RedirectStandardError = true;
                    psi.RedirectStandardOutput = true;
                    psi.UseShellExecute = false;

                    var process = Process.Start(psi);
                    process.WaitForExit();

                    if (process.ExitCode == -1)
                    {
                        var error = process.StandardError.ReadToEnd();
                        throw new Exception(error);
                    }

                    var file = GetOutputSvgFile(_profile);
                    var viewer = new SvgViewer();
                    viewer.LoadImage(file);
                    viewer.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Reading profile file failed!");
            }
        }

        private void ProcessProfile(IProgress progress, Profile profile)
        {
            if (StartFunction == null)
            {
                return;
            }

            if (_model == null)
            {
                var parser = new ProfileParser(progress);

                // Mark initial set of functions as filtered.
                // Only functions that are included here can be edited (included / excluded) later.
                var eventStream = parser.ParseEventStream(profile.EventFile, _idToFunctionInfo);

                // Call graph variations for the given entry function(s)
                _model = SequenceModel.FromEventStream(eventStream, StartFunction);

                // If we change the start function later we have to rebuild the model. This means reading the large profile file.
            }
        }
    }
}