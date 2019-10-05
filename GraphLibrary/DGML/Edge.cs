using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphLibrary.Dgml
{
    /// <summary>
    /// Represents an edge to be rendered.   
    /// </summary>
    internal class Edge
    {
        public Edge(string sourceNode, string targetNode)
        {
            Source = sourceNode;
            Target = targetNode;
        }

        public Edge(string sourceNode, string targetNode, string category)
        {
            Source = sourceNode;
            Target = targetNode;
            Category = category;
        }

        public string Source { get; set; }
        public string Target { get; set; }

        public string Category { get; set; }
        
    }
}
