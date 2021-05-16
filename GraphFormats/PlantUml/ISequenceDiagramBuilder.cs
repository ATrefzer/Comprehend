namespace GraphFormats.PlantUml
{
    public interface ISequenceDiagramBuilder
    {
        void AddEdge(IFunctionPresentation sourceNode, IFunctionPresentation targetNode);
        void AddEdge(IFunctionPresentation sourceNode, IFunctionPresentation targetNode, string category);

        void AddCategory(string category, string property, string value);

        void Activate(IFunctionPresentation node);
        void Deactivate(IFunctionPresentation node);
        void NewObject(IFunctionPresentation targetNode);
        string Build();
    }
}