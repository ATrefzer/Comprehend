namespace GraphFormats.Dgml
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

        public string Source { get; }
        public string Target { get; }

        public string Category { get; }
    }
}