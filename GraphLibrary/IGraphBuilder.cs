using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphLibrary
{
    public interface IGraphBuilder
    {
        void AddEdge(string sourceNode, string targetNode);
        void AddEdge(string sourceNode, string targetNode, string category);

        void AddCategory(string category, string property, string value);
    }
}
