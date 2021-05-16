using System.Collections.Generic;
using System.Linq;
using Launcher.Profiler;

namespace Launcher.Models
{
    /// <summary>
    ///     Given a start function find all traces where this start function is called.
    ///     <see cref="SequenceVariations" />
    ///     The traces contain all(!) invocations.
    ///     This model requires new FunctionCall instances whenever a function is called. This is different
    ///     in the call graph model.
    /// </summary>
    internal class CallTree
    {
        protected readonly Dictionary<ulong, Stack<FunctionCall>> TidToStack =
            new Dictionary<ulong, Stack<FunctionCall>>();

        private CallTree()
        {
        }

        /// <summary>
        ///     The final found sequences starting from the entry function.
        ///     Each entry is a trace of the start function.
        /// </summary>
        public List<FunctionCall> SequenceVariations { get; private set; }

        public static CallTree FromEventStream(IEnumerable<ProfilerEvent> stream,
            FunctionInfo entryFunction)
        {
            var model = new CallTree();
            model.FromEventStream_(stream, entryFunction);
            return model;
        }


        public void FromEventStream_(IEnumerable<ProfilerEvent> eventStream, FunctionInfo entryFunction)
        {
            var sequenceVariations = new List<FunctionCall>();
            Clear();

            foreach (var entry in eventStream)
            {
                if (entry.Token == Tokens.TokenEnter || entry.Token == Tokens.TokenTailCall)
                {
                    var enterFunc = CreateNewFunctionCall(entry);

                    var stack = FindStackByThreadId(entry.ThreadId);

                    var isEntry = enterFunc.FullName == entryFunction.FullName;
                    if (stack == null && isEntry)
                    {
                        // Create stack only if we find an entry function
                        stack = GetOrCreateStackByThreadId(entry.ThreadId);
                    }

                    if (stack == null)
                    {
                        // Continue searching the profile until we find the start function.
                        continue;
                    }


                    var activeFunc = GetActiveFunction(stack);
                    activeFunc?.Children.Add(enterFunc);

                    if (entry.Token == Tokens.TokenEnter)
                    {
                        // This is the new active function. Note that for a tail call we do not receive a 
                        // Tokens.TokenLeave! So the active function stays intact.
                        stack.Push(enterFunc);
                    }
                }
                else if (entry.Token == Tokens.TokenLeave)
                {
                    var stack = FindStackByThreadId(entry.ThreadId);
                    if (stack != null)
                    {
                        // We are currently tracking a sequence
                        var activeFunc = GetActiveFunction(stack);

                        if (activeFunc != null && activeFunc.FullName == entry.Func.FullName)
                        {
                            // Deactivate this functions
                            var leaveFunc = stack.Pop();

                            // Sequence complete
                            if (!stack.Any())
                            {
                                sequenceVariations.Add(leaveFunc);

                                // Stop tracking calls.
                                TidToStack.Remove(entry.ThreadId);
                            }
                        }
                    }
                }
                else if (entry.Token == Tokens.TokenDestroyThread)
                {
                    if (TidToStack.ContainsKey(entry.ThreadId))
                    {
                        TidToStack.Remove(entry.ThreadId);

                        // TODO close all open methods(!)
                    }
                }
            }

            SequenceVariations = sequenceVariations;
            Clear();
            
        }


        protected void Clear()
        {
            TidToStack.Clear();
        }

        protected FunctionCall GetActiveFunction(Stack<FunctionCall> stack)
        {
            if (stack == null)
            {
                return null;
            }

            // Find active function
            FunctionCall activeFunc = null;
            if (stack.Count > 0)
            {
                activeFunc = stack.Peek();
            }

            return activeFunc;
        }

        private Stack<FunctionCall> GetOrCreateStackByThreadId(ulong threadId)
        {
            // Find the correct thread such that we find the correct parent function.
            if (!TidToStack.TryGetValue(threadId, out var stack))
            {
                stack = new Stack<FunctionCall>();
                TidToStack.Add(threadId, stack);
            }

            return stack;
        }

        private Stack<FunctionCall> FindStackByThreadId(ulong threadId)
        {
            // Find the correct thread such that we find the correct parent function.
            TidToStack.TryGetValue(threadId, out var stack);
            return stack;
        }

        private FunctionCall CreateNewFunctionCall(ProfilerEvent entry)
        {
            FunctionCall newFunc;
            newFunc = new FunctionCall(entry.Func);
            return newFunc;
        }
    }
}