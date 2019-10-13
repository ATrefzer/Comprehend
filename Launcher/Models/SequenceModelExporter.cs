using System;
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
            var variations = model.SequenceVariations;
            if (variations.Count == 0)
            {
                throw new Exception("No Sequence to generate!");
            }

            var sequence = variations.First();

            foreach (var call in sequence)
            {
                builder.AddEdge(call.Item1.Name, call.Item2.Name);
            }
        }
    }
}