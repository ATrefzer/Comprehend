using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace GraphLibrary.Dgml
{
    /// <summary>
    /// Builder class to create a directed graph file to be processed with Visual Studio.
    /// </summary>
    public class DgmlFileBuilder
    {
        public DgmlFileBuilder()
        {
            _edges = new List<Edge>();
            _nodeNamesToId = new Dictionary<string, string>();
            _idCounter = 0;
        }

        /// <summary>
        /// Adds an edge to the output file. Node are collected automatically and don't need to be added separately.
        /// </summary>        
        public void AddEdge(string sourceNode, string targetNode)
        {
            // Convert the node names to node Ids.        
            string sourceNodeId = GetOrCreateNodeId(sourceNode);
            string targetNodeId = GetOrCreateNodeId(targetNode);

            _edges.Add(new Edge(sourceNodeId, targetNodeId));
        }

        private bool IsNonPrintableCharacter(char candidate)
        {
            return (candidate < 0x20 || candidate > 127);
        }

        /// <summary>
        /// Creates the output file.
        /// </summary>        
        public void WriteOutput(string path)
        {
            using (var writer = XmlWriter.Create(path))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("DirectedGraph", "http://schemas.microsoft.com/vs/2009/dgml");

                // Write section for the nodes
                writer.WriteStartElement("Nodes");
                foreach (var node in _nodeNamesToId)
                {
                    writer.WriteStartElement("Node");
                    writer.WriteAttributeString("Id", node.Value);

                    var escaped = node.Key;
                    if (node.Key.Any(IsNonPrintableCharacter))
                    {
                        escaped = "Cryptic_" + node.Value;
                    }
                    writer.WriteAttributeString("Label", escaped);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();

                // Write section for the edges.
                writer.WriteStartElement("Links");
                foreach (Edge edge in _edges)
                {
                    writer.WriteStartElement("Link");
                    writer.WriteAttributeString("Source", edge.Source);
                    writer.WriteAttributeString("Target", edge.Target);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();

                writer.WriteEndElement(); // DirectedGraph
                writer.WriteEndDocument();
            }
        }

        /// <summary>
        /// Returns an identifier for the given node name.
        /// Remember that nodes are collected by adding edges.
        /// If the id for the given node name already exists it is returned. Otherwise
        /// a new identifier is created.        
        /// </summary>        
        private string GetOrCreateNodeId(string nodeName)
        {
            string id = null;

            _nodeNamesToId.TryGetValue(nodeName, out id);

            if (id == null)
            {
                // Node is not present so far. Create a new id.
                id = Convert.ToString(_idCounter++);
                _nodeNamesToId.Add(nodeName, id);
            }

            return id;
        }

        /// <summary>
        /// Edges
        /// </summary>
        private readonly List<Edge> _edges;

        /// <summary>
        /// Nodes name (lower case) are mapped to an unique node id
        /// </summary>
        private readonly Dictionary<string, string> _nodeNamesToId;
        
        private int _idCounter;
    }
}