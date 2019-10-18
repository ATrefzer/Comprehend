namespace GraphLibrary
{
    // TODO parsing outside.
    public interface ISequenceBuilder
    {
        void AddEdge(string sourceNode, string targetNode);
        void AddEdge(string sourceNode, string targetNode, string category);

        void AddCategory(string category, string property, string value);

        void Activate(string node);
        void Deactivate(string node);
    }
}