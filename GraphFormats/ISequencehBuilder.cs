namespace GraphFormats
{
    public interface ISequenceBuilder
    {
        void AddEdge(IFunctionPresentation sourceNode, IFunctionPresentation targetNode);
        void AddEdge(IFunctionPresentation sourceNode, IFunctionPresentation targetNode, string category);

        void AddCategory(string category, string property, string value);

        void Activate(IFunctionPresentation node);
        void Deactivate(IFunctionPresentation node);
        void NewObject(IFunctionPresentation targetNode);
    }
}