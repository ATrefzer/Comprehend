namespace GraphFormats
{
    public interface IGraphBuilder
    {
        void AddEdge(string sourceNode, string targetNode);
        void AddEdge(string sourceNode, string targetNode, string category);

        void AddCategory(string category, string property, string value);
        void AddNode(string node);
    }
}