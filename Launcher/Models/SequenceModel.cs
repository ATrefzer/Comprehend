using System.Collections.Generic;
using System.Linq;

using Launcher.Profiler;

namespace Launcher.Models
{
    /// <summary>
    /// Pre-filtered to call stacks for a single function, but contains all hidden outgoing calls.
    /// As helper for following processing steps we add a token when a function is finished.
    /// Otherwise we cannot distinguish when a function is finished.
    /// This token has only the first item present, the second is null.
    /// </summary>
    internal class SequenceModel : BaseModel
    {
        private static Dictionary<ulong, List<(FunctionCall, FunctionCall)>> _threadIdToSequence;

        private static Filter _filter = Filter.Default();

        private SequenceModel(List<List<(FunctionCall, FunctionCall)>> sequence)
        {
            SequenceVariations = sequence;
        }

        public List<List<(FunctionCall, FunctionCall)>> SequenceVariations { get; }

        public static SequenceModel FromEventStream(IEnumerable<ProfilerEvent> eventStream, FunctionInfo entryFunction)
        {
            var sequenceVariations = new List<List<(FunctionCall, FunctionCall)>>();
            Clear();

            foreach (var entry in eventStream)
            {
                if (entry.Token == Tokens.TokenEnter)
                {
                    var enterFunc = GetEnteredFunction(entry);

                    var stack = FindStackByThreadId(entry.ThreadId);

                    var isEntry = enterFunc.Name == entryFunction.Name;
                    if (stack == null && isEntry)
                    {
                        // Create stack only if we find an entry function
                        stack = GetOrCreateStackByThreadId(entry.ThreadId);
                    }

                    var activeFunc = GetActiveFunction(stack);
                    if (activeFunc != null)
                    {
                        var sequence = GetOrCreateSequence(entry.ThreadId);

                        if (ReferenceEquals(activeFunc, enterFunc))
                        {
                            activeFunc.Recursive = true;
                        }
                        else
                        {
                            sequence.Add((activeFunc, enterFunc));
                        }
                    }

                    // Here the stack may be null if the entry function is not found so var.
                    // This is the new active function 
                    stack?.Push(enterFunc);
                }
                else if (entry.Token == Tokens.TokenLeave)
                {
                    var stack = FindStackByThreadId(entry.ThreadId);
                    if (stack != null)
                    {
                        // We are currently tracking a sequence
                        var activeFunc = GetActiveFunction(stack);

                        if (activeFunc != null && activeFunc.Name == entry.Func.Name)
                        {
                            // Deactivate this functions
                            var leaveFunc = stack.Pop();

                            var sequence = GetOrCreateSequence(entry.ThreadId);

                            // (*) Add a token in the sequence to signal when a function is done.
                            sequence.Add((leaveFunc, null));

                            // Sequence complete
                            if (!stack.Any())
                            {
                                sequenceVariations.Add(sequence);

                                // Stop tracking calls.
                                _threadIdToSequence.Remove(entry.ThreadId);
                                _tidToStack.Remove(entry.ThreadId);
                            }
                        }
                        else
                        {
                            // Ignore. We did not start recording at the time.
                        }
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
                    if (_tidToStack.ContainsKey(entry.ThreadId))
                    {
                        _tidToStack.Remove(entry.ThreadId);
                    }

                    if (_threadIdToSequence != null && _threadIdToSequence.ContainsKey(entry.ThreadId))
                    {
                        _threadIdToSequence.Remove(entry.ThreadId);
                    }

                    // TODO close all open methods(!)
                }
            }

            var model = new SequenceModel(sequenceVariations);
            Clear();
            return model;
        }

        private static List<(FunctionCall, FunctionCall)> GetOrCreateSequence(ulong threadId)
        {
            if (_threadIdToSequence == null)
            {
                _threadIdToSequence = new Dictionary<ulong, List<(FunctionCall, FunctionCall)>>();
            }

            if (!_threadIdToSequence.TryGetValue(threadId, out var sequence))
            {
                sequence = new List<(FunctionCall, FunctionCall)>();
                _threadIdToSequence.Add(threadId, sequence);
            }

            return sequence;
        }
    }
}