namespace GraphFormats.Graphwiz
{
    /// <summary>
    /// Represents an edge to be rendered by Graphwiz.
    /// It corresponds to an input instruction of the form: "node1" -> "node2"
    /// </summary>
    internal class Edge
    {
        private string _source;
        private string _target;

        public Edge(string sourceNode, string targetNode)
        {
            Source = sourceNode;
            Target = targetNode;
        }

        public string Source
        {
            get => _source;

            // To ensure that the path does not contain characters not understood by dot we quote the name.
            set => _source = "\"" + value + "\"";
        }

        public string Target
        {
            get => _target;

            // To ensure that the path does not contain characters not understood by dot we quote the name.
            set => _target = "\"" + value + "\"";
        }
    }
}