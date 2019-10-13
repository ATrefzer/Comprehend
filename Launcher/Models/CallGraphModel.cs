﻿using System.Collections.Generic;
using System.Linq;

using Launcher.Profiler;

namespace Launcher.Models
{
    internal class CallGraphModel : BaseModel
    {

        public CallGraphModel(List<FunctionCall> model)
        {
            AllFunctions = model;
        }

        public List<FunctionCall> AllFunctions { get; }


        public static CallGraphModel FromEventStream(IEnumerable<ProfilerEvent> stream)
        {
            Clear(); // TODO inside base

            //var numEvent = 0;

            foreach (var entry in stream)
            {
                if (entry.Token == Tokens.TokenEnter)
                {
                    // 1. Mark as child of the active function
                    // 2. Push as active function

                    var stack = GetOrCreateStackByThreadId(entry.ThreadId);
                    var enterFunc = GetEnteredFunction(entry);
                    var activeFunc = GetActiveFunction(stack);

                    if (activeFunc != null)
                    {
                        if (ReferenceEquals(activeFunc, enterFunc))
                        {
                            activeFunc.Recursive = true;
                        }

                        activeFunc.Children.Add(enterFunc);
                        enterFunc.Parents.Add(activeFunc);

                        if (!enterFunc.IsFiltered)
                        {
                            // We cant remove the ancestors of enterFunc because they contain at least
                            // one visible child.
                            MarkAllAncestorOfVisibleChild(activeFunc);
                        }
                    }

                    // This is the new active function 
                    // Stack tracks recursive calls. But they do not appear in the model later.
                    stack.Push(enterFunc);
                }
                else if (entry.Token == Tokens.TokenLeave)
                {
                    var stack = GetOrCreateStackByThreadId(entry.ThreadId);
                    var activeFunc = GetActiveFunction(stack);

                    if (activeFunc != null && activeFunc.Name == entry.Func.Name)
                    {
                        stack.Pop();

                        // Reduce memory by cleaning up while we process the event stream.
                        // We remove all functions that are hidden and only call hidden functions!
                        CleanupHiddenCalls(_functions, activeFunc);
                    }
                    else
                    {
                        // Ignore. We did not start recording at the time.
                    }
                }
                else if (entry.Token == Tokens.TokenTailCall)
                {
                    if (!_functions.TryGetValue(entry.Func.Id, out var newFunc))
                    {
                        newFunc = CreateFunctionCall(entry);
                        newFunc.TailCall = true;
                        _functions.Add(newFunc.Id, newFunc);
                    }

                    newFunc.TailCall = true;
                }
                else if (entry.Token == Tokens.TokenDestroyThread)
                {
                    _tidToStack.Remove(entry.ThreadId);

                    // TODO close all open methods(!)
                }
            }

            var model = new CallGraphModel(_functions.Values.ToList());
           Clear(); // TODO inside base
            return model;
        }




        private static void MarkAllAncestorOfVisibleChild(FunctionCall activeFunc)
        {
            var ancestors = activeFunc.GetAncestorChain();
            foreach (var ancestor in ancestors)
            {
                ancestor.HasVisibleChildren = true;
            }

            //var processedParent = new HashSet<ulong>();

            //// Mark all parents that the have at least one visible child
            //var parents = new Queue<FunctionCall>();
            //parents.Enqueue(activeFunc);
            //while (parents.Any())
            //{
            //    var parent = parents.Dequeue();
            //    processedParent.Add(parent.Id);
            //    if (parent.HasVisibleChildren == false)
            //    {
            //        parent.HasVisibleChildren = true;

            //        foreach (var ancestor in parent.Parents)
            //        {
            //            if (!processedParent.Contains(ancestor.Id))
            //            {
            //                parents.Enqueue(ancestor);
            //            }
            //        }
            //    }
            //}
        }

        private static void CleanupHiddenCalls(Dictionary<ulong, FunctionCall> allFunctions, FunctionCall exitFunc)
        {
            if (CanRemove(exitFunc))
            {
                allFunctions.Remove(exitFunc.Id);

                // Cleanup all calls to this function. There is nothing worth down there.
                foreach (var parent in exitFunc.Parents)
                {
                    parent.Children.Remove(exitFunc);
                }

                exitFunc.Parents.Clear();
            }
        }

        private static bool CanRemove(FunctionCall exitFunc)
        {
            return exitFunc.IsFiltered && exitFunc.HasVisibleChildren == false;
        }

    
    }
}