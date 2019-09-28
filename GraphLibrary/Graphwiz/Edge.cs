using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CppProjectAssistant.Graphwiz
{
    /// <summary>
    /// Represents an edge to be rendered by Graphwiz.
    /// It corresponds to an input instruction of the form: "node1" -> "node2"
    /// </summary>
    internal class Edge
    {
        public Edge(string sourceNode, string targetNode)
        {
            Source = sourceNode;
            Target = targetNode;
        }

        private string _source;
        public string Source
        {
            get { return _source; }

            // To ensure that the path does not contain characters not understood by dot we quote the name.
            set { _source = "\"" + value + "\""; }
        }
        private string _target;
        public string Target
        {
            get { return _target; }

            // To ensure that the path does not contain characters not understood by dot we quote the name.
            set { _target = "\"" + value + "\""; }
        }
    }
}
