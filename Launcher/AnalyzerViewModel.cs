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
    internal class Trace
    {
        private readonly string _directory;

        public Trace(string directory, string baseFile)
        {
            _directory = directory;
            BaseFile = baseFile;
        }

        public string BaseFile { get; }

        public string IndexFile => Path.Combine(_directory, BaseFile + ".index");
        public string EventFile => Path.Combine(_directory, BaseFile + ".profile");

        public override string ToString()
        {
            return BaseFile;
        }
    }

    public class TracesArg
    {
        public string Path { get; set; }
    }

    internal class AnalyzerViewModel : INotifyPropertyChanged
    {
        private Trace _selectedTrace;

        private readonly HashSet<FunctionCall> _processed = new HashSet<FunctionCall>();

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

            var builder = new DgmlFileBuilder();
            builder.AddCategory("indirect", "StrokeDashArray", "1 1");
            BuildDgml(builder, model);

            builder.WriteOutput(Path.Combine(WorkingDirectory, SelectedTrace + ".graph.dgml"));
        }

        private void BuildDgml(DgmlFileBuilder builder, CallGraphModel model)
        {
            _processed.Clear();
            foreach (var func in model.AllFunctions.Where(f => !f.IsHidden))
            {
                // Start with all visible functions and add them to the graph
                BuildDgml_Iter(builder, null, func);
            }
        }

        private void BuildDgml_Iter(DgmlFileBuilder builder, FunctionCall lastVisibleAncestor, FunctionCall target)
        {

            var toProcess = new Queue<(FunctionCall, FunctionCall)>();
            toProcess.Enqueue((lastVisibleAncestor, target));

            while (toProcess.Any())
            {
                (lastVisibleAncestor,target) = toProcess.Dequeue();


                if (_processed.Contains(target))
                {
                    continue;
                }

                _processed.Add(target);


                if (lastVisibleAncestor != null && !target.IsHidden)
                {
                    if (lastVisibleAncestor.Children.Contains(target))
                    {
                        // Direct call
                        builder.AddEdge(lastVisibleAncestor.Name, target.Name);
                    }
                    else
                    {
                        // Indirect call (mark as dashed line)
                        builder.AddEdge(lastVisibleAncestor.Name, target.Name, "indirect");
                    }
                }


                
                if (!target.IsHidden)
                {
                    // New visible parent for the children
                    lastVisibleAncestor = target;
                }

                foreach (var call in target.Children)
                {
                    toProcess.Enqueue((lastVisibleAncestor, call));
                }
            }


         

            // Assumption: We start with the first visible parent. Anything hidden above is ignored.

          

        }

        private void BuildDgml(DgmlFileBuilder builder, FunctionCall lastVisibleAncestor, FunctionCall target)
        {
            if (_processed.Contains(target))
            {
                return;
            }

            _processed.Add(target);

            // Assumption: We start with the first visible parent. Anything hidden above is ignored.

            if (lastVisibleAncestor != null && !target.IsHidden)
            {
                if (lastVisibleAncestor.Children.Contains(target))
                {
                    // Direct call
                    builder.AddEdge(lastVisibleAncestor.Name, target.Name);
                }
                else
                {
                    // Indirect call (mark as dashed line)
                    builder.AddEdge(lastVisibleAncestor.Name, target.Name, "indirect");
                }
            }

            if (!target.IsHidden)
            {
                // New visible parent for the children
                lastVisibleAncestor = target;
            }

            foreach (var call in target.Children)
            {
                BuildDgml(builder, lastVisibleAncestor, call);
            }
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