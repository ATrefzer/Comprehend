using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GraphLibrary.PlantUml
{
    /// <summary>
    /// Dumps profiler info to plantuml format.
    /// We receive graph node information as a function in the form module!namespace_and_type.function.
    /// </summary>
    public class PlantUmlBuilder : IGraphBuilder
    {
        // SourceType, TargetType, Method
        private readonly List<(string, string, string)> _orderedEdges = new List<(string, string, string)>();

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
            var sourceParts = SplitFullName(sourceNode);
            var targetParts = SplitFullName(targetNode);

            _orderedEdges.Add((sourceParts.TypeName, targetParts.TypeName, targetParts.Function));
        }

        public void AddEdge(string sourceNode, string targetNode, string category)
        {
            // TODO category

            // A node is a function that calls another function!
            var sourceParts = SplitFullName(sourceNode);
            var targetParts = SplitFullName(targetNode);

            _orderedEdges.Add((sourceParts.TypeName, targetParts.TypeName, targetParts.Function));
        }

        public void AddCategory(string category, string property, string value)
        {
        }

        public void WriteOutput(string file)
        {
            using (var writer = new StreamWriter(file, false))
            {
                writer.WriteLine("@startuml _title_");
                writer.WriteLine("hide footbox");

                //writer.WriteLine("actor client");

                if (_orderedEdges.Any())
                {
                    foreach (var edge in _orderedEdges)
                    {
                        if (edge.Item1 != null)
                        {
                            // For the first call we do not have a 
                            writer.WriteLine($"{edge.Item1} -> {edge.Item2} : {edge.Item3}");
                        }
                    }

                    //participant p #lightblue

                    writer.WriteLine("@enduml");
                }
            }
        }

        public class Parts
        {
            public string Module;
            public string TypeName;
            public string Function;
        }
    }
}