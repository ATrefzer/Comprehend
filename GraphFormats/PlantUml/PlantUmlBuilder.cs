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


        private readonly Dictionary<string, string> _asyncAwaitGeneratedFunctionMappings = new Dictionary<string, string>();

        public string Title { get; set; } = "_title";


        public void AddEdge(IFunction sourceNode, IFunction targetNode)
        {
            // A node is a function that calls another function!
            var edge = CreateEdge(sourceNode, targetNode);
            if (edge == null)
            {
                return;
            }

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

        public void Activate(IFunction targetNode)
        {
            // Activate a target
            var edge = CreateActivation(targetNode);
            _orderedEdge.Add(edge);
        }

        public void NewObject(IFunction targetNode)
        {
            // Activate a target
            var edge = CreateNewObject(targetNode);
            _orderedEdge.Add(edge);
        }

        public void Deactivate(IFunction sourceNode)
        {
            // Deactivate the source
            var edge = CreateDeactivation(sourceNode);
            _orderedEdge.Add(edge);
        }

        public void AddEdge(IFunction sourceNode, IFunction targetNode, string category)
        {
            // A node is a function that calls another function!
            var edge = CreateEdge(sourceNode, targetNode);
            if (edge == null)
            {
                return;
            }

            edge.Color = FindProperty(category, "color");
            _orderedEdge.Add(edge);
        }

        public void WriteOutput(string file)
        {
            using (var writer = new StreamWriter(file, false))
            {
                writer.WriteLine($"@startuml {Title}");
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

                                continue;
                            }
                            else if (edge.IsDeactivation)
                            {
                                if (edge.SourceType != null)
                                {
                                    writer.WriteLine($"deactivate {CleanUpInvalidChars(edge.SourceType)}");
                                }

                                continue;
                            }
                            else if (edge.IsCreation)
                            { 
                                writer.WriteLine("create " + edge.TargetType);
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(edge.Color))
                                {
                                    writer.WriteLine($"{CleanUpInvalidChars(edge.SourceType)} -> {CleanUpInvalidChars(edge.TargetType)} : {CleanUpInvalidChars(edge.TargetFunction)}");
                                }
                                else
                                {
                                    writer.WriteLine($"{CleanUpInvalidChars(edge.SourceType)} -[{edge.Color}]-> {CleanUpInvalidChars(edge.TargetType)} : {CleanUpInvalidChars(edge.TargetFunction)}");
                                }
                            }
                        }
                    }

                    //participant p #lightblue

                    writer.WriteLine("@enduml");
                }
            }
        }

        private string FindProperty(string category, string property)
        {
            if (_categories == null)
            {
                return null;
            }

            if (_categories.TryGetValue(category, out var properties))
            {
                if (properties.TryGetValue(property, out var value))
                {
                    return value;
                }
            }

            return null;
        }


        private Edge CreateDeactivation(IFunction sourceNode)
        {
            // After source called the last function we deactivate it.

            var edge = new Edge();
            edge.SourceType = CleanUpInvalidChars(sourceNode.TypeName);
            edge.IsDeactivation = true;
            return edge;
        }

        private Edge CreateActivation(IFunction targetNode)
        {
            var edge = new Edge();
            edge.TargetType = CleanUpInvalidChars(targetNode.TypeName);
            edge.IsActivation = true;
            return edge;
        }

        private Edge CreateNewObject(IFunction targetNode)
        {
            var edge = new Edge();
            edge.TargetType = CleanUpInvalidChars(targetNode.TypeName);
            edge.IsCreation = true;
            return edge;
        }

        private Edge CreateEdge(IFunction sourceNode, IFunction targetNode)
        {
            
            var edge = new Edge();
            edge.SourceType = CleanUpInvalidChars(sourceNode.TypeName);
            edge.TargetType = CleanUpInvalidChars(targetNode.TypeName);
            edge.TargetFunction = CleanUpInvalidChars(targetNode.Function);
            return edge;
        }

       

        private string CleanUpInvalidChars(string input)
        {
            var clean = input.Replace('`', '_').Replace('<', '_').Replace('>', '_').Replace('-', '_');
            return clean;
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
            public bool IsCreation { get; set; }
        }
    }
}