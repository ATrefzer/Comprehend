using GraphFormats;

namespace GraphLibrary
{
    public interface ISequenceBuilder
    {
        void AddEdge(IFunction sourceNode, IFunction targetNode);
        void AddEdge(IFunction sourceNode, IFunction targetNode, string category);

        void AddCategory(string category, string property, string value);

        void Activate(IFunction node);
        void Deactivate(IFunction node);
        void NewObject(IFunction targetNode);
    }
}