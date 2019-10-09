using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GraphLibrary.Graphwiz
{
    /// <summary>
    /// Builder class to create a directed graph file to be processed with Graphwiz.
    /// </summary>
    internal class DotFileBuilder
    {
        private readonly List<Edge> _edges;

        public DotFileBuilder()
        {
            _edges = new List<Edge>();
        }

        public void AddEdge(string sourceNode, string targetNode)
        {
            _edges.Add(new Edge(sourceNode, targetNode));
        }

        public void WriteOutput(string path)
        {
            using (var writer = new StreamWriter(path, false, Encoding.ASCII))
            {
                writer.WriteLine("digraph G");

                // graph [size="100.3,50.3", ranksep=0.5, nodesep=0.1, overlap=false, start=2 ratio=2]

                writer.WriteLine("{");

                foreach (var edge in _edges)
                {
                    var output = string.Format("{0} -> {1}", edge.Source, edge.Target);
                    writer.WriteLine(output);
                }

                writer.WriteLine("}");
            }
        }
    }
}