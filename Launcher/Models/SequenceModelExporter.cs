using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using GraphLibrary;

using SharpVectors.Dom.Svg;

namespace Launcher.Models
{
    /// <summary>
    /// Uses the builder to generate a UML sequence diagram.
    /// Hidden functions calls are excluded and indirect calls marked accordingly.
    /// Note: op_Implicit calls the ctor. So ctor may not be the first method called on an object.
    /// If this op_Implicit is present I cannot use "create" with plantuml.
    /// It requires the following call after "create" to be the first one (ctor, new)
    /// Therefore I filter op_Implicit in the filter file to get rid of it immediately.
    /// </summary>
    internal class SequenceModelExporter
    {
        private Stack<FunctionPresentation> _visibleParents;
        private ISequenceBuilder _builder;

        public void Export(SequenceModel model, ISequenceBuilder builder)
        {
            // From last visible parent skipping hidden calls to another visible call.
            _visibleParents = new Stack<FunctionPresentation>();
            _builder = builder;

            // TODO many variations are not evaluated
            builder.AddCategory("indirect", "color", "#0000FF");

            var variations = model.SequenceVariations;
            if (variations.Count == 0)
            {
                throw new Exception("No Sequence to generate!");
            }

            var sequence = variations.Last();
            var presentationSequence = sequence.Select(tuple => (new FunctionPresentation(tuple.Item1), new FunctionPresentation(tuple.Item2))).ToList();

            // Optional to get rid of the async await state machine objects
            MergeAsyncAwaitInternals(presentationSequence);
            InsertDummyCaller(presentationSequence);

            // TODO Debug
            var lines = presentationSequence.Select(tuple => $"{tuple.Item1.FullName}->{tuple.Item2?.FullName}");
            File.WriteAllLines("d:\\lines.txt", lines);

            int lineNumber = 0;
            foreach (var (source, target) in presentationSequence)
            {
                lineNumber++;

                if (target == null || target.IsNull)
                {
                    ExitFunction(source);
                    continue;
                }

                var lastVisibleParent = _visibleParents.Any() ? _visibleParents.Peek() : null;

                if (!source.IsFiltered && !target.IsFiltered)
                {
                    InvokeFunctionDirectly(source, target);
                }
                else if (lastVisibleParent != null && !target.IsFiltered)
                {
                    Debug.Assert(source.IsFiltered);
                    InvokeFunctionIndirectly(lastVisibleParent, target);
                }
                else if (source.IsFiltered && target.IsFiltered)
                {
                    // Ignore hidden calls
                }
                else
                {
                    // Visible -> Hidden
                }
            }
        }


        private static void InsertDummyCaller(List<(FunctionPresentation, FunctionPresentation)> presentationSequence)
        {
            if (presentationSequence.Any())
            {
                // Insert dummy call to entry function to activate the entry function
                var client = new FunctionPresentation { TypeName = "Client", IsFiltered = false, IsCtor = false };
                presentationSequence.Insert(0, (client, presentationSequence.First().Item1));
            }
        }

        private void InvokeFunctionIndirectly(FunctionPresentation lastVisibleParent, FunctionPresentation target)
        {
            InvokeFunction(lastVisibleParent, target, "indirect");
        }

        private void InvokeFunctionDirectly(FunctionPresentation source, FunctionPresentation target)
        {
            InvokeFunction(source, target);
        }

        private void InvokeFunction(FunctionPresentation source, FunctionPresentation target, string category = null)
        {
            if (target.IsCtor && source.TypeName != target.TypeName) // Static method calling ctor like DelegateCommand.New
            {
                // New swim lane before the call is done
                _builder.NewObject(target);
            }

            if (category == null)
            {
                _builder.AddEdge(source, target);
            }
            else
            {
                _builder.AddEdge(source, target, "indirect");
            }

            _visibleParents.Push(target);

            if (!target.IsCtor)
            {
                // Active the target after the call is done
                _builder.Activate(target);
            }
        }

        private void ExitFunction(FunctionPresentation source)
        {
            // Input stream notification that a function ended.
            // Signals exit of Item1. This helps to track activations.
            // Deactivate source node when last function was done.

            if (!source.IsFiltered)
            {
                // We only activate visible functions 
                _builder.Deactivate(source);

                // Exiting a visible parent function
                var exited = _visibleParents.Pop();
                Debug.Assert(exited.FullName == source.FullName);
            }
        }

        private void MergeAsyncAwaitInternals(List<(FunctionPresentation, FunctionPresentation)> presentationSequence)
        {
            // An async await call is detected when a method creates an state machine object (ctor)
            // This state machine object has a certain naming format.
            // For example:
            //
            // lib.Module!MyClass.MyMethodAsync calls lib.Module!<MyMethodAsync>d__46..ctor

            // In this case I reuse the type name of the caller for the state machine type name.
            // In other words I merge the state machine object code into the caller.

            // If I do this I have to skip the ctor call to the state machine object. I merge the state machine 
            // functions into the existing(!) caller.

            var typeRenaming = new Dictionary<string, string>();

            foreach (var (source, target) in presentationSequence)
            {
                if (!source.IsNull && !target.IsNull && target.IsCtor)
                {
                    if (target.TypeName.Contains($"<{source.Function}>"))
                    {
                        // Creation of a async state machine
                        // Merge the state machine object code into the original caller.
                        if (!typeRenaming.ContainsKey(target.FullName))
                        {
                            typeRenaming.Add(target.FullName, source.TypeName);
                        }
                    }
                }

                MergeIfStateMachine(source, typeRenaming);

                // Note: Target function may be null if it just shows termination of a function.
                MergeIfStateMachine(target, typeRenaming);
            }
        }

        /// <summary>
        /// Type of the state machine object is replaced by its caller.
        /// So we merge the state machine into its caller.
        /// </summary>
        private void MergeIfStateMachine(FunctionPresentation stateMachineFunc, IReadOnlyDictionary<string, string> fullNameToTypeName)
        {
            if (!stateMachineFunc.IsNull && fullNameToTypeName.TryGetValue(stateMachineFunc.FullName, out var typeName))
            {
                stateMachineFunc.TypeName = typeName;

                if (stateMachineFunc.IsCtor)
                {
                    stateMachineFunc.IsFiltered = true;

                    // Can no longer be a ctor.
                    // We construct the async state machine but we merge this class into the existing caller.
                    stateMachineFunc.IsCtor = false;
                }
            }
        }
    }
}