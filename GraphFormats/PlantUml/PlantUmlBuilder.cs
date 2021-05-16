using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphFormats.PlantUml
{
    /// <summary>
    ///     Api to build a plantuml text..
    /// </summary>
    public class PlantUmlBuilder : ISequenceDiagramBuilder
    {
        private readonly Dictionary<string, Dictionary<string, string>> _categories = new Dictionary<string, Dictionary<string, string>>();

        // SourceType, TargetType, Method
        private readonly List<Edge> _orderedEdge = new List<Edge>();

        public PlantUmlBuilder(string title)
        {
            Title = title;
        }

        public string Title { get; set; } = "_title";


        public void AddEdge(IFunctionPresentation sourceNode, IFunctionPresentation targetNode)
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

        public void Activate(IFunctionPresentation targetNode)
        {
            // Activate a target
            var edge = CreateActivation(targetNode);
            _orderedEdge.Add(edge);
        }

        public void NewObject(IFunctionPresentation targetNode)
        {
            // Activate a target
            var edge = CreateNewObject(targetNode);
            _orderedEdge.Add(edge);
        }

        public void Deactivate(IFunctionPresentation sourceNode)
        {
            // Deactivate the source
            var edge = CreateDeactivation(sourceNode);
            _orderedEdge.Add(edge);
        }

        public void AddEdge(IFunctionPresentation sourceNode, IFunctionPresentation targetNode, string category)
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

        public string Build()
        {
            var writer = new StringBuilder();
            {
                writer.AppendLine($"@startuml {Title}");
                writer.AppendLine("hide footbox");

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
                                    writer.AppendLine($"activate {CleanUpInvalidChars(edge.TargetType)}");
                                }

                                continue;
                            }

                            if (edge.IsDeactivation)
                            {
                                if (edge.SourceType != null)
                                {
                                    writer.AppendLine($"deactivate {CleanUpInvalidChars(edge.SourceType)}");
                                }

                                continue;
                            }

                            if (edge.IsCreation)
                            {
                                writer.AppendLine("create " + edge.TargetType);
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(edge.Color))
                                {
                                    writer.AppendLine(
                                        $"{CleanUpInvalidChars(edge.SourceType)} -> {CleanUpInvalidChars(edge.TargetType)} : {CleanUpInvalidChars(edge.TargetFunction)}");
                                }
                                else
                                {
                                    writer.AppendLine(
                                        $"{CleanUpInvalidChars(edge.SourceType)} -[{edge.Color}]-> {CleanUpInvalidChars(edge.TargetType)} : {CleanUpInvalidChars(edge.TargetFunction)}");
                                }
                            }
                        }
                    }

                    //participant p #lightblue

                    writer.AppendLine("@enduml");
                }

                return writer.ToString();
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


        private Edge CreateDeactivation(IFunctionPresentation sourceNode)
        {
            // After source called the last function we deactivate it.

            var edge = new Edge();
            edge.SourceType = CleanUpInvalidChars(sourceNode.TypeName);
            edge.IsDeactivation = true;
            return edge;
        }

        private Edge CreateActivation(IFunctionPresentation targetNode)
        {
            var edge = new Edge();
            edge.TargetType = CleanUpInvalidChars(targetNode.TypeName);
            edge.IsActivation = true;
            return edge;
        }

        private Edge CreateNewObject(IFunctionPresentation targetNode)
        {
            var edge = new Edge();
            edge.TargetType = CleanUpInvalidChars(targetNode.TypeName);
            edge.IsCreation = true;
            return edge;
        }

        private Edge CreateEdge(IFunctionPresentation sourceNode, IFunctionPresentation targetNode)
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