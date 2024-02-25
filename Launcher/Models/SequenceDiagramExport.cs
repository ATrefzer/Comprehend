using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using GraphFormats.PlantUml;

namespace Launcher.Models
{
    /// <summary>
    ///     Uses the builder to generate a UML sequence diagram.
    ///     Hidden functions calls are excluded and indirect calls marked accordingly.
    ///     Note: op_Implicit calls the ctor. So ctor may not be the first method called on an object.
    ///     If this op_Implicit is present I cannot use "create" with plantuml.
    ///     It requires the following call after "create" to be the first one (ctor, new)
    ///     Therefore I filter op_Implicit in the filter file to get rid of it immediately.
    /// </summary>
    internal class SequenceDiagramExport
    {
        private readonly ISequenceDiagramBuilder _builder;
        private Stack<TreeCall> _visibleParents;

        public SequenceDiagramExport(string title, bool simplify)
        {
            _builder = new PlantUmlBuilder(title, simplify);
        }

        public void Export(string outputPath, TreeCall funcCall)
        {
            Export(funcCall);
            var text = _builder.Build();
            File.WriteAllText(outputPath, text);
        }

        private void Export(TreeCall funcCall)
        {
            // From last visible parent skipping hidden calls to another visible call.
            _visibleParents = new Stack<TreeCall>();

            _builder.AddCategory("indirect", "color", "#0000FF");


            if (funcCall == null)
            {
                throw new Exception("No Sequence to generate!");
            }


            // TODO
            // Optional to get rid of the async await state machine objects
            // MergeAsyncAwaitInternals(presentationSequence);


            // Dummy Actor calling the traced function. Included for sure!
            var actor = TreeCall.CreateActor();
            Call(actor, funcCall);
        }


        private void Call(TreeCall source, TreeCall target)
        {
            if (source.IsIncluded && target.IsIncluded)
            {
                InvokeTargetDirectly(source, target);
            }
            else if (!source.IsIncluded && target.IsIncluded)
            {
                InvokeTargetIndirectly(_visibleParents.Peek(), target);
            }


            foreach (var child in target.Children)
            {
                Call(target, child);
            }


            EndFunction(target);
        }

        private void InvokeTargetIndirectly(TreeCall lastVisibleParent, TreeCall target)
        {
            InvokeFunction(lastVisibleParent, target, "indirect");
        }

        private void InvokeTargetDirectly(IFunctionPresentation source, TreeCall target)
        {
            InvokeFunction(source, target);
        }

        private void InvokeFunction(IFunctionPresentation source, TreeCall target, string category = null)
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

        private void EndFunction(TreeCall target)
        {
            if (target.IsIncluded)
            {
                // We only activate visible functions 
                _builder.Deactivate(target);

                // Exiting a visible parent function
                var exited = _visibleParents.Pop();
                Debug.Assert(exited.Function == target.Function);
            }
        }

/*
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
                    stateMachineFunc.IsBanned = true;

                    // Can no longer be a ctor.
                    // We construct the async state machine but we merge this class into the existing caller.
                    stateMachineFunc.IsCtor = false;
                }
            }
        }
        */
    }
}