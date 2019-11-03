using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Documents;

using GraphLibrary;

namespace Launcher.Models
{
    /// <summary>
    /// Uses the builder to generate a UML sequence diagram.
    /// Hidden functions calls are excluded and indirect calls marked accordingly.
    /// </summary>
    internal class SequenceModelExporter
    {
        public void Export(SequenceModel model, ISequenceBuilder builder)
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


            if (sequence.Any())
            {
                var first = sequence.FirstOrDefault();
                builder.AddEdge("Client.dontCare", first.Item1.Name);

                // 
                if (first.Item2 != null)
                    builder.Activate(first.Item2.Name);
            }

            foreach (var call in sequence)
            {
               

                if (!call.Item1.IsFiltered)
                {
                    // Can it be that simple?
                    lastVisibleParent = call.Item1;
                }


                if (call.Item2 == null)
                {
                    // Signals exit of Item1. This helps to track activations.
                    // Deactivate source node when last function was done.
                    if (!call.Item1.IsFiltered)
                        builder.Deactivate(call.Item1.Name);
                    continue;
                }


                if (!call.Item1.IsFiltered && !call.Item2.IsFiltered)
                {
                    builder.AddEdge(call.Item1.Name, call.Item2.Name);
                    
                    // Active the target.
                    builder.Activate(call.Item2.Name);
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