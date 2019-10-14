using System;
using System.Diagnostics;
using System.Linq;

using GraphLibrary;

namespace Launcher.Models
{
    internal class SequenceModelExporter
    {
        public void Export(SequenceModel model, IGraphBuilder builder)
        {
            // TODO builder interface is useless here
            // TODO process all variations


            // From last visible parent skipping hidden calls to another visible 
            // call is presented differently.
            FunctionCall lastVisibleParent = null;

            var variations = model.SequenceVariations;
            if (variations.Count == 0)
            {
                throw new Exception("No Sequence to generate!");
            }

            var sequence = variations.First();

            foreach (var call in sequence)
            {
                if (!call.Item1.IsFiltered)
                {
                    // Can it be that simple?
                    lastVisibleParent = call.Item1;
                }


                if (!call.Item1.IsFiltered && !call.Item2.IsFiltered)
                {
                    builder.AddEdge(call.Item1.Name, call.Item2.Name);
                }
                else if (lastVisibleParent != null && !call.Item2.IsFiltered)
                {
                    // Skip hidden parts
                    
                    builder.AddEdge(call.Item1.Name, call.Item2.Name, "indirect"); // TODO AddCategory
                }
                else if (call.Item1.IsFiltered && call.Item2.IsFiltered)
                {
                    continue;
                }
                else
                {
                    // Visible -> Hidden
                    // Debug.Assert(false);
                }
                
                
            }
        }
    }
}