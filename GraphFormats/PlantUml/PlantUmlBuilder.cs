using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphFormats.PlantUml
{
    /// <summary>
    ///     Api to build a plantuml text.
    /// </summary>
    public class PlantUmlBuilder : ISequenceDiagramBuilder
    {
        private readonly HashSet<string> _aliases = new HashSet<string>();
        private readonly Dictionary<string, Dictionary<string, string>> _categories = new Dictionary<string, Dictionary<string, string>>();

        // SourceType, TargetType, Method
        private readonly List<Edge> _orderedEdge = new List<Edge>();
        private readonly bool _simplify;

        private readonly string _title;

        private readonly Dictionary<string, string> _typeToAlias = new Dictionary<string, string>();

        public PlantUmlBuilder(string title, bool simplify)
        {
            _simplify = simplify;
            _title = title ?? "_title";
        }


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
                writer.AppendLine($"@startuml {_title}");
                writer.AppendLine("!theme plain");
                writer.AppendLine("hide footbox");

                if (_orderedEdge.Any())
                {
                    foreach (var edge in _orderedEdge)
                    {
                        if (edge.IsActivation)
                        {
                            if (edge.TargetType != null)
                            {
                                writer.AppendLine($"activate {MapTypeToAlias(edge.TargetType)}");
                            }

                            continue;
                        }

                        if (edge.IsDeactivation)
                        {
                            if (edge.SourceType != null)
                            {
                                writer.AppendLine($"deactivate {MapTypeToAlias(edge.SourceType)}");
                            }

                            continue;
                        }

                        if (edge.IsCreation)
                        {
                            writer.AppendLine("create " + MapTypeToAlias(edge.TargetType));
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(edge.Color))
                            {
                                writer.AppendLine(
                                    $"{MapTypeToAlias(edge.SourceType)} -> {MapTypeToAlias(edge.TargetType)} : {CleanUpInvalidChars(edge.TargetFunction)}");
                            }
                            else
                            {
                                writer.AppendLine(
                                    $"{MapTypeToAlias(edge.SourceType)} -[{edge.Color}]-> {MapTypeToAlias(edge.TargetType)} : {CleanUpInvalidChars(edge.TargetFunction)}");
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
            edge.SourceType = MapTypeToAlias(sourceNode.TypeName);
            edge.IsDeactivation = true;
            return edge;
        }

        private Edge CreateActivation(IFunctionPresentation targetNode)
        {
            var edge = new Edge();
            edge.TargetType = MapTypeToAlias(targetNode.TypeName);
            edge.IsActivation = true;
            return edge;
        }

        private Edge CreateNewObject(IFunctionPresentation targetNode)
        {
            var edge = new Edge();
            edge.TargetType = MapTypeToAlias(targetNode.TypeName);
            edge.IsCreation = true;
            return edge;
        }

        private Edge CreateEdge(IFunctionPresentation sourceNode, IFunctionPresentation targetNode)
        {
            var edge = new Edge();
            edge.SourceType = MapTypeToAlias(sourceNode.TypeName);
            edge.TargetType = MapTypeToAlias(targetNode.TypeName);
            edge.TargetFunction = CleanUpInvalidChars(targetNode.Function);
            return edge;
        }

        /// <summary>
        ///     Get rid of namespaces to draw a more compact diagram.
        ///     If a this causes a conflict with another alias the original
        ///     type name is returned (cleaned)
        /// </summary>
        private string TryStripNamespace(string typeName)
        {
            var alias = typeName;

            var lastDot = typeName.LastIndexOf('.');
            if (lastDot >= 0)
            {
                alias = typeName.Substring(lastDot + 1);
                if (!_aliases.Add(alias))
                {
                    // Someone already uses this alias, cant simplify
                    alias = typeName;
                }
            }

            return alias;
        }

        private string MapTypeToAlias(string type)
        {
            if (!_typeToAlias.ContainsKey(type))
            {
                if (_simplify)
                {
                    _typeToAlias[type] = TryStripNamespace(CleanUpInvalidChars(type));
                }
                else
                {
                    _typeToAlias[type] = CleanUpInvalidChars(type);
                }
            }

            return _typeToAlias[type];
        }

        /// <summary>
        ///     Plantuml does not accept all characters in type or function names
        /// </summary>
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