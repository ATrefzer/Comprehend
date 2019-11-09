using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Security.Cryptography;
using System.Windows.Documents;

using GraphFormats;

using GraphLibrary;

using Launcher.Profiler;

namespace Launcher.Models
{
    /// <summary>
    /// Uses the builder to generate a UML sequence diagram.
    /// Hidden functions calls are excluded and indirect calls marked accordingly.
    /// </summary>
    internal class SequenceModelExporter
    {
        [DebuggerDisplay("Type: {TypeName} Func={Function} Ctor={IsCtor}")]
        class FunctionPresentation : IFunction
        {
            private readonly FunctionCall _call;

            public FunctionPresentation(FunctionCall call)
            {
                _call = call;

                if (call == null)
                {
                    IsNull = true;
                    IsCtor = false;
                    return;
                }

                IsNull = false;
                TypeName = call.TypeName;
                Function = call.Function;
                IsCtor = call.IsCtor;
                IsRecursive = call.IsRecursive;
                FullName = call.FullName;

                // Underlying object is modified by the filter UI
                IsFiltered = call.IsFiltered;

            }

            public string FullName { get; }

            public bool IsRecursive { get; set; }

            public string TypeName { get; set; }
            public string Function { get; }

            public bool IsCtor { get; set; }

            public bool IsNull { get; }

            
            public bool IsFiltered { get; set; }
        }




        public void Export(SequenceModel model, ISequenceBuilder builder)
        {
            // TODO many variations are not evaluated
            builder.AddCategory("indirect", "color", "#0000FF");

            var variations = model.SequenceVariations;
            if (variations.Count == 0)
            {
                throw new Exception("No Sequence to generate!");
            }

            // TODO only first variation is processed.
            var sequence = variations.First();
            var  presentationSequence = sequence.Select(tuple => (new FunctionPresentation(tuple.Item1), new FunctionPresentation(tuple.Item2))).ToList();

            // Optional to get rid of the async await state machine objects
            MergeAsyncAwaitInternas(presentationSequence);
            InsertDummyCaller(presentationSequence);

            // From last visible parent skipping hidden calls to another visible call.
            var visibleParents = new Stack<FunctionPresentation>();
          

            //var file = new StreamWriter("d:\\debug.txt", false);
            //int debug = 0;

            foreach (var call in presentationSequence)
            {
                //debug++;
                //file.WriteLine($"{debug} + {call.Item1.FullName} -> {call.Item2?.FullName}");
                //file.Flush();

                if (call.Item2 == null || call.Item2.IsNull)
                {
                    // Input stream notification that a function ended.
                    // Signals exit of Item1. This helps to track activations.
                    // Deactivate source node when last function was done.
                    
                    if (!call.Item1.IsFiltered)
                    {
                        // We only activate visible functions 
                        builder.Deactivate(call.Item1);

                        // Exiting a visible parent function
                        var exited = visibleParents.Pop();
                        Debug.Assert(exited.FullName == call.Item1.FullName);
                       
                    }
                    continue;
                }




                var lastVisibleParent = visibleParents.Any() ? visibleParents.Peek() : null;

                if (!call.Item1.IsFiltered && !call.Item2.IsFiltered)
                {
                    if (call.Item2.IsCtor)
                    {
                        // New swim lane
                        builder.NewObject(call.Item2);
                    }

                    builder.AddEdge(call.Item1, call.Item2);
                    visibleParents.Push(call.Item2);

                    if (!call.Item2.IsCtor)
                    {
                        // Active the target.
                        builder.Activate(call.Item2);
                    }
                  
                }
                else if (lastVisibleParent != null && !call.Item2.IsFiltered)
                {
                    Debug.Assert(call.Item1.IsFiltered);
                    // Skip hidden parts
                    
                    if (call.Item2.IsCtor)
                    {
                        // New swim lane before the call is done
                        builder.NewObject(call.Item2);
                    }

                    // TODO indirect call?
                   // builder.AddEdge(call.Item1, call.Item2, "indirect"); 
                   builder.AddEdge(lastVisibleParent, call.Item2, "indirect"); 
                   visibleParents.Push(call.Item2);

                    if (!call.Item2.IsCtor)
                    {
                        // Active the target after the call is done
                        builder.Activate(call.Item2);
                    }
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

            //file.Close();
        }

     

        private static void InsertDummyCaller(List<(FunctionPresentation, FunctionPresentation)> presentationSequence)
        {
            if (presentationSequence.Any())
            {
                // Insert dummy call to entry function to activate the entry function
                var first = presentationSequence.FirstOrDefault();
                var dummyInfo = new FunctionInfo(0, "!Client.dontCare.dontCare", true, false);
                var client = new FunctionCall(dummyInfo);
                presentationSequence.Insert(0, (new FunctionPresentation(client), first.Item1));
            }
        }

        private static void MergeAsyncAwaitInternas(List<(FunctionPresentation, FunctionPresentation)> presentationSequence)
        {
            // Presentation sequence
            // Function calls other function with following type name:
            //

            // aufruf der Form
            //  lib.SD.BaseCidDevice.BL.dll!lib.SD.BaseCidDevice.BL.Online.BaseDeviceDownload.DownloadCoreAsync

            // Wir wssen lib.SD.BaseCidDevice.BL.dll!<DownloadCoreAsync>d__46..ctor ist die statemachine und entspricht eigentlich dem Typen des aufrufers!!!!

            // => Dann benutze den Typennamen des Aufrufers. (Methode ruft sich selber auf???)

            // Lege neue sequenz an mit IFUnction objeckte!

            // Simplify presentation model
            var typeRenamings = new Dictionary<string, string>();

            foreach (var call in presentationSequence)
            {
                if (!call.Item1.IsNull && !call.Item2.IsNull && call.Item2.IsCtor)
                {
                    if (call.Item2.TypeName.Contains($"<{call.Item1.Function}>"))
                    {
                        // Creation of a async state machine
                        // Merge the state machine object code into the original caller.
                        if (!typeRenamings.ContainsKey(call.Item2.FullName))
                        {
                            typeRenamings.Add(call.Item2.FullName, call.Item1.TypeName);
                        }
                    }
                }

                string typeName;
                string fullName;

                // Note: Second function may be null if it just shows termination of a function.
                fullName = call.Item2.FullName;
                if (!call.Item2.IsNull && typeRenamings.TryGetValue(fullName, out typeName))
                {
                    call.Item2.TypeName = typeName;

                    // Can no longer be a ctor. We construct the async state machine but we merge this class with the existing caller(!)
                    if (call.Item2.IsCtor)
                    {
                        call.Item2.IsFiltered = true;
                        call.Item2.IsCtor = false;
                    }
                }

                fullName = call.Item1.FullName;
                if (typeRenamings.TryGetValue(fullName, out typeName))
                {
                    call.Item1.TypeName = typeName;

                    // Can no longer be a ctor. We construct the async state machine but we merge this class with the existing caller(!)
                    if (call.Item1.IsCtor)
                    {
                        call.Item1.IsFiltered = true;
                        call.Item1.IsCtor = false;
                    }
                }
            }
        }
    }
}