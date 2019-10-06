using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Input;

using GraphLibrary.Dgml;

using Launcher.Models;

using Prism.Commands;

namespace Launcher
{

    internal class AnalyzerViewModel : INotifyPropertyChanged
    {
        private Trace _selectedTrace;

     
        public AnalyzerViewModel()
        {
            GenerateFilteredGraphCommand = new DelegateCommand(ExecuteGenerateFilteredGraph);

            //GenerateFilteredGraphCommand = new DelegateCommand(ExecuteGenerateFilteredGraph, IsTraceSelected);
            EditFilterCommand = new DelegateCommand(ExecuteEditFilter);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public string WorkingDirectory { get; set; }
        public ICommand GenerateFilteredGraphCommand { get; }
        public ICommand EditFilterCommand { get; }

        public Trace SelectedTrace
        {
            get => _selectedTrace;
            set
            {
                if (_selectedTrace != value)
                {
                    _selectedTrace = value;
                    OnPropertyChanged();

                    //CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public ObservableCollection<Trace> AvailableTraces { get; } = new ObservableCollection<Trace>();

        public void RefreshAvailableTraces(object sender, TracesArg args)
        {
            WorkingDirectory = args.Path;
            RefreshAvailableTraces();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool IsTraceSelected()
        {
            return SelectedTrace != null;
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
        private void ExecuteGenerateFilteredGraph()
        {
            var selection = SelectedTrace;
            if (selection == null)
            {
                Debug.Assert(false);
                return;
            }

            var filter = Filter.FromFile(GetFilterFilePath());

            //var filter = Filter.Default();
            var parser = new ProfileParser();
            var eventStream = parser.Parse(selection.IndexFile, selection.EventFile);
            var model = CallGraphModel.FromEventStream(eventStream, filter);

            var visibleFuncs = model.AllFunctions.Where(f => f.IsHidden == false).ToList();

            var exporter = new CallGraphExporter();
            exporter.Export(model, Path.Combine(WorkingDirectory, SelectedTrace + ".graph.dgml"));
        }

        private void RefreshAvailableTraces()
        {
            AvailableTraces.Clear();

            if (!Directory.Exists(WorkingDirectory))
            {
                return;
            }

            var files = Directory.EnumerateFiles(WorkingDirectory, "*.index");
            foreach (var file in files)
            {
                var fi = new FileInfo(file);
                var baseName = fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length);
                var trace = new Trace(WorkingDirectory, baseName);
                AvailableTraces.Add(trace);
            }
        }
    }
}