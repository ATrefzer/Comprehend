﻿using System;
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
    internal class CallGraphViewModel : INotifyPropertyChanged, IGenerator
    {
        private readonly BackgroundExecutionService _backgroundService;
        private Profile _selectedProfile;
        private Dictionary<ulong, FunctionInfo> _idToFunctionInfo;


        public CallGraphViewModel(BackgroundExecutionService backgroundService)
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

                    //CommandManager.InvalidateRequerySuggested();
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


        private CallGraphModel ProcessProfile(IProgress progress, Profile profile)
        {
            var parser = new ProfileParser(progress);

            // Add filter here only for performance.
            var eventStream = parser.ParseEventStream(profile.EventFile, _idToFunctionInfo);

            var model = CallGraphModel.FromEventStream(eventStream);
            return model;
        }

        private string GetOutputDgmlFile(Profile profile)
        {
            return Path.Combine(WorkingDirectory, profile + ".graph.dgml");
        }

        private void OpenMethodChooserAsync()
        {

            var profile = SelectedProfile;
            if (profile == null)
            {
                Debug.Assert(false);
            }

            
            var preFilter = Filter.FromFile(GetFilterFilePath());
            var parser = new ProfileParser();
            _idToFunctionInfo = parser.ParseIndex(profile.IndexFile, preFilter);
            
            
            // All functions that are included according to the pre filter file
            // These functions can be hidden or made visible
            var preSelection = _idToFunctionInfo.Values.Where(info => !info.IsFiltered).Select(info => new FunctionInfoViewModel(info));


            var setupWindow = new MethodChooserView();
            var viewModel = new MethodChooserViewModel(_backgroundService, WorkingDirectory, this);
            viewModel.HasStartFunction = false;
            viewModel.Initialize(preSelection);
            setupWindow.DataContext = viewModel;
            setupWindow.Show();
        }

     
        public async Task ExecuteGenerate(FunctionInfo startFunction)
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
                    var msagl = new MsaglGrapBuilder();
                    exporter.Export(model, msagl);
                    msagl.ShowResult();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Reading profile file failed!");
            }
        }

    }
}