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

        public string Source { get; set; }
        public string Target { get; set; }
        
    }
}
