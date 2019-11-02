using System;
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

using Process = Launcher.Profiler.Process;

namespace Launcher
{
    class SequenceDiagramSetupViewModel : INotifyPropertyChanged
    {
        private readonly BackgroundExecutionService _backgroundService;
        private readonly string _workingDirectory;
        private Profile _profile;
        private Dictionary<ulong, FunctionInfo> _idToFunctionInfo;

        /// <summary>
        ///  Entry function(s) with all pre-filtered methods including those hidden functions necessary for indirect calls
        /// </summary>
        private SequenceModel _model;

        public ICommand GenerateCommand { get; }

        public SequenceDiagramSetupViewModel(BackgroundExecutionService backgroundService, string workingDirectory)
        {
            _backgroundService = backgroundService;
            _workingDirectory = workingDirectory;
            GenerateCommand = new DelegateCommand(Generate);
        }

        private string GetOutputPlantumlFile(Profile profile)
        {
            return Path.Combine(profile.Directory , profile.BaseFile + ".plantuml");
        }

        private string GetOutputSvgFile(Profile profile)
        {
            return  Path.Combine(profile.Directory , profile.BaseFile + ".svg");
        }

        private async void Generate()
        {
            try
            {
                // TODO atr
                _model = null;
                await _backgroundService.RunWithProgress(progress => ProcessProfile(progress, _profile));

                // Model is available

                var exporter = new SequenceModelExporter();

                // TODO open new user interface
                string fullPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                if (_model != null)
                {
                    var builder = new PlantUmlBuilder();
                    builder.Title = _profile.BaseFile;
                    builder.AddCategory("indirect", "color", "#0000FF");
                    exporter.Export(_model, builder);
                    builder.WriteOutput(GetOutputPlantumlFile(_profile));

                    var exeDir = Path.GetDirectoryName( fullPath );

                    var psi = new ProcessStartInfo();
                    psi.FileName = "java.exe";
                    psi.Arguments = "-jar " + Path.Combine(exeDir, "Dependencies", "plantuml.jar ") + GetOutputPlantumlFile(_profile) + " -tsvg";
                    psi.CreateNoWindow = true;
                    psi.RedirectStandardError = true;
                    psi.RedirectStandardOutput = true;
                    psi.UseShellExecute = false;

                    var process = System.Diagnostics.Process.Start(psi);
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

        public void Initialize(Profile profile, Filter preFilter)
        {
            // Get all available function, pre-filtered
            _profile = profile;

            var parser = new ProfileParser();
            _idToFunctionInfo = parser.ParseIndex(profile.IndexFile, preFilter);

            AllPreFilteredFunctions = new ObservableCollection<FunctionInfo>(_idToFunctionInfo.Values.Distinct());
            Debug.Assert(AllPreFilteredFunctions.Count == _idToFunctionInfo.Count);


        }


        public ObservableCollection<FunctionInfo> AllPreFilteredFunctions { get; set; }
        


        private FunctionInfo _entryFunction;

        public FunctionInfo EntryFunction
        {
            get { return _entryFunction; }
            set
            {
                if (value == _entryFunction)
                {
                    return;
                }

                _model = null;
                _entryFunction = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanRender));
            }
        }

        public bool CanRender
        {
            get { return EntryFunction != null; }
        }

        private void ProcessProfile(IProgress progress, Profile profile)
        {
            if (EntryFunction == null)
            {
                return;
            }

            if (_model == null)
            {
                var parser = new ProfileParser(progress);

                // Add filter here only for performance.
                var eventStream = parser.ParseEventStream(profile.EventFile, _idToFunctionInfo);

                // Call graph variations for the given entry function(s)
                _model = SequenceModel.FromEventStream(eventStream, EntryFunction);

                // TODO Entry function is no longer changeble.
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}