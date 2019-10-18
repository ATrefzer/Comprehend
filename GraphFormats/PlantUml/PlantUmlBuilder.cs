using System.Collections.Generic;
using System.IO;
using System.Linq;

using GraphLibrary;

namespace GraphFormats.PlantUml
{
    /// <summary>
    /// Dumps profiler info to plantuml format.
    /// We receive graph node information as a function in the form module!namespace_and_type.function.
    /// </summary>
    public class PlantUmlBuilder : ISequenceBuilder
    {

        // SourceType, TargetType, Method
        private readonly List<Edge> _orderedEdge = new List<Edge>();

        private readonly Dictionary<string, Dictionary<string, string>> _categories = new Dictionary<string, Dictionary<string, string>>();

        public Parts SplitFullName(string name)
        {
            var parts = new Parts();

            var lastDot = name.LastIndexOf('.');
            parts.Function = name.Substring(lastDot + 1);

            var bang = name.IndexOf('!');
            if (bang >= 0)
            {
                parts.Module = name.Substring(0, bang);
                parts.TypeName = name.Substring(bang + 1, lastDot - bang - 1);
            }
            else
            {
                parts.Module = "unknown";
                parts.TypeName = name.Substring(0, lastDot);
            }

            return parts;
        }

        public void AddEdge(string sourceNode, string targetNode)
        {
            // A node is a function that calls another function!
            var edge = CreateEdge(sourceNode, targetNode);
            _orderedEdge.Add(edge);
        }

        // Supported: color
        public void AddCategory(string category, string property, string value)
        {
            if (!_categories.TryGetValue(category, out var properties))
            {
                properties = new Dictionary<string, string>();
                _categories.Add(category, properties);
            }

            if (!properties.ContainsKey(property))
            {
                properties.Add(property, value);
            }
            else
            {
                properties[property] = value;
            }
        }

        public void Activate(string targetNode)
        {
            // Activate a target
            var edge = CreateActivation(targetNode);
            _orderedEdge.Add(edge);
        }

        public void Deactivate(string sourceNode)
        {
            // Deactivate the source
            var edge = CreateDeactivation(sourceNode);
            _orderedEdge.Add(edge);
        }

        public void AddEdge(string sourceNode, string targetNode, string category)
        {

            // A node is a function that calls another function!
            var edge = CreateEdge(sourceNode, targetNode);
            edge.Color = FindProperty(category, "color");
            _orderedEdge.Add(edge);
        }
        string FindProperty(string category, string property)
        {
            if (_categories == null)
                return null;

            if (_categories.TryGetValue(category, out var properties))
            {
                if (properties.TryGetValue(property, out var value))
                {
                    return value;
                }
            }

            return null;
        }
      

        public void WriteOutput(string file)
        {
            using (var writer = new StreamWriter(file, false))
            {
                writer.WriteLine("@startuml _title_");
                writer.WriteLine("hide footbox");

                //writer.WriteLine("actor client");

                if (_orderedEdge.Any())
                {
                    foreach (var edge in _orderedEdge)
                    {
                        //if (edge.SourceType != null)
                        {
                            if (edge.IsActivation)
                            {
                                if (edge.TargetType != null)
                                {
                                    writer.WriteLine($"activate {CleanUpInvalidChars(edge.TargetType)}");
                                }
                            }
                            else if (edge.IsDeactivation)
                            {
                                if (edge.SourceType != null)
                                writer.WriteLine($"deactivate {CleanUpInvalidChars(edge.SourceType)}");
                            }
                            else if (string.IsNullOrEmpty(edge.Color))
                            {
                                writer.WriteLine($"{CleanUpInvalidChars(edge.SourceType)} -> {CleanUpInvalidChars(edge.TargetType)} : {CleanUpInvalidChars(edge.TargetFunction)}");
                            }
                            else
                            {
                                writer.WriteLine($"{CleanUpInvalidChars(edge.SourceType)} -[{edge.Color}]-> {CleanUpInvalidChars(edge.TargetType)} : {CleanUpInvalidChars(edge.TargetFunction)}");
                            }
                        }
                    }

                    //participant p #lightblue

                    writer.WriteLine("@enduml");
                }
            }
        }



        private Edge CreateDeactivation(string sourceNode)
        {
            // After source called the last function we deactivate it.
            var sourceParts = SplitFullName(sourceNode);

            var edge = new Edge();
            edge.SourceType = sourceParts.TypeName;
            edge.IsDeactivation = true;
            return edge;
        }

        private Edge CreateActivation(string targetNode)
        {
            var targetParts = SplitFullName(targetNode);

            var edge = new Edge();
            edge.TargetType = targetParts.TypeName;
            edge.IsActivation = true;
            return edge;
        }

        private Edge CreateEdge(string sourceNode, string targetNode)
        {
            var sourceParts = SplitFullName(sourceNode);
            var targetParts = SplitFullName(targetNode);

            var edge = new Edge();
            edge.SourceType = sourceParts.TypeName;
            edge.TargetType = targetParts.TypeName;
            edge.TargetFunction = targetParts.Function;
            return edge;
        }

        private string CleanUpInvalidChars(string input)
        {
            return input.Replace('`', '_').Replace('<', '_').Replace('>', '_');
        }

        public class Parts
        {
            public string Module;
            public string TypeName;
            public string Function;
        }

        private class Edge
        {

            public string TargetFunction { get; set; }
            public string TargetType { get; set; }
            public string SourceType { get; set; }

            // Optional
            public string Color { get; set; }
            public bool IsActivation { get; set; }
            public bool IsDeactivation { get; set; }
        }
    }
}