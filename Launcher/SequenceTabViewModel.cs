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

using GraphFormats.PlantUml;

using Launcher.Execution;
using Launcher.Models;
using Launcher.Profiler;

using Prism.Commands;

using Process = System.Diagnostics.Process;

namespace Launcher
{
    internal class SequenceTabViewModel : INotifyPropertyChanged, IGenerator
    {
        private readonly BackgroundExecutionService _backgroundService;
        private Profile _selectedProfile;


        public SequenceTabViewModel(BackgroundExecutionService backgroundService)
        {
            _backgroundService = backgroundService;
            OpenMethodChooserCommand = new DelegateCommand(() => OpenMethodChooserAsync());

            EditFilterCommand = new DelegateCommand(ExecuteEditFilter);
        }


        public event PropertyChangedEventHandler PropertyChanged;
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

        private void OpenMethodChooserAsync()
        {
            var profile = SelectedProfile;
            Debug.Assert(profile != null);

            _fullModel = null;
            var preFilter = Filter.FromFile(GetFilterFilePath());
            var parser = new ProfileParser();
            _idToFunctionInfo = parser.ParseIndex(profile.IndexFile, preFilter);

            // All functions that are included according to the pre filter file
            // These functions can be hidden or made visible
            var preSelection = _idToFunctionInfo.Values.Where(info => !info.IsFiltered).Select(info => new FunctionInfoViewModel(info));


            var setupWindow = new MethodChooserView();
            var viewModel = new MethodChooserViewModel(_backgroundService, WorkingDirectory, this);
            viewModel.SetInstructions(Launcher.Resources.Resources.InstructionSequence);
            viewModel.Initialize(preSelection);
            setupWindow.DataContext = viewModel;
            setupWindow.ShowDialog();

        }

        /// <summary>
        ///  Entry function(s) with all pre-filtered methods including those hidden functions necessary for indirect calls
        /// </summary>
        private SequenceModel _fullModel;
     
        
        private string GetPlantUmlTitle(Profile profile)
        {
            return profile.BaseFile.Replace(".", "_");
        }

        
        private string GetOutputPlantumlFile(Profile profile)
        {
            return Path.Combine(profile.Directory, profile.BaseFile.Replace(".", "_") + ".plantuml");
        }


        private string GetOutputSvgFile(Profile profile)
        {
            return Path.Combine(profile.Directory, profile.BaseFile.Replace(".", "_") + ".svg");
        }


        private FunctionInfo _startFunction;
        private Dictionary<ulong, FunctionInfo> _idToFunctionInfo;




        public async Task ExecuteGenerate(FunctionInfo startFunction)
        {
            if (_selectedProfile == null)
                return;

            if (_startFunction != startFunction)
            {
                _startFunction = startFunction;

                // Need to process the profile again
                _fullModel = null;
            }
    
            try
            {
               

                await _backgroundService.RunWithProgress(progress => ProcessProfile(progress, _selectedProfile));

                // Model is available with all called functions. No filter applied yet.

                var exporter = new SequenceModelExporter();

                var fullPath = Assembly.GetExecutingAssembly().Location;
                if (_fullModel != null)
                {
                    var builder = new PlantUmlBuilder();
                    builder.Title = GetPlantUmlTitle(_selectedProfile);
                    exporter.Export(_fullModel, builder);
                    builder.WriteOutput(GetOutputPlantumlFile(_selectedProfile));

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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Reading profile file failed!");
            }
        }

        public HashSet<FunctionInfo> GetModelFunctions()
        {
            if (_fullModel == null)
                return new HashSet<FunctionInfo>();

            var allFunctionsInModel = new HashSet<FunctionInfo>();
            var allCalls = _fullModel.SequenceVariations.SelectMany(variation => variation);
            foreach (var tpl in allCalls)
            {
                if (tpl.Item1 != null) allFunctionsInModel.Add(tpl.Item1.Info);
                if (tpl.Item2 != null) allFunctionsInModel.Add(tpl.Item2.Info);
            }

            return allFunctionsInModel ;
        }


        private void ProcessProfile(IProgress progress, Profile profile)
        {

            if (_startFunction == null)
            {
                return;
            }


            if (_fullModel == null)
            {
                var parser = new ProfileParser(progress);

                // Mark initial set of functions as filtered.
                // Only functions that are included here can be edited (included / excluded) later.
                var eventStream = parser.ParseEventStream(profile.EventFile, _idToFunctionInfo);

                // Call graph variations for the given entry function(s)
                _fullModel = SequenceModel.FromEventStream(eventStream, _startFunction);

                // If we change the start function later we have to rebuild the model. This means reading the large profile file.
            }
        }

       
    }
}