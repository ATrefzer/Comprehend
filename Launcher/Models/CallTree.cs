using System.Collections.Generic;
using System.Linq;
using Launcher.Profiler;

namespace Launcher.Models
{
    /// <summary>
    ///     Given a start function find all traces where this start function is called.
    ///     <see cref="SequenceVariations" />
    ///     The traces contain all(!) invocations from the starting point..
    ///     This model requires new FunctionCall instances whenever a function is called. This is different
    ///     in the call graph model.
    ///
    ///     Noteworthy things:
    ///     - A tail call has no associated leave token.
    ///     - You cannot compare function names due to method overloading. Always use the Ids
    ///     - If a method is closed that is not the active function on the stack an exception or jump happened.
    /// 
    /// </summary>
    internal class CallTree
    {
        private readonly Dictionary<ulong, Stack<FunctionCall>> _tidToStack =
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


        private void FromEventStream_(IEnumerable<ProfilerEvent> eventStream, FunctionInfo entryFunction)
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
                        while (stack.Count > 0 && stack.Peek().Id != entry.Func.Id)
                        {
                            // Exception handling!
                            stack.Pop();
                        }
                        
                        var activeFunc = GetActiveFunction(stack);
                        if (activeFunc != null)
                        {
                            // We are currently tracking a sequence.
                            var leaveFunc = stack.Pop();
                            if (!stack.Any())
                            {
                                // Sequence complete
                                sequenceVariations.Add(leaveFunc);

                                // Stop tracking calls.
                                _tidToStack.Remove(entry.ThreadId);
                            }
                        }
                    }
                }
                else if (entry.Token == Tokens.TokenDestroyThread)
                {
                    if (_tidToStack.ContainsKey(entry.ThreadId))
                    {
                        _tidToStack.Remove(entry.ThreadId);

                        // Open calls are closed inherently.
                    }
                }
            }

            SequenceVariations = sequenceVariations;
            Clear();
        }


        private void Clear()
        {
            _tidToStack.Clear();
        }

        private FunctionCall GetActiveFunction(Stack<FunctionCall> stack)
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
            if (!_tidToStack.TryGetValue(threadId, out var stack))
            {
                stack = new Stack<FunctionCall>();
                _tidToStack.Add(threadId, stack);
            }

            return stack;
        }

        private Stack<FunctionCall> FindStackByThreadId(ulong threadId)
        {
            // Find the correct thread such that we find the correct parent function.
            _tidToStack.TryGetValue(threadId, out var stack);
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