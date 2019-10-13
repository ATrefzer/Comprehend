using System;
using System.Collections.Generic;
using System.Linq;

using Launcher.Profiler;

namespace Launcher.Models
{
    internal class SequenceModel : BaseModel
    {
        // TODO allow variations

        private static Dictionary<ulong, List<(FunctionCall, FunctionCall)>> _threadIdToSequence;
      
        public List<List<(FunctionCall, FunctionCall)>> SequenceVariations {
            get; 
        }

        private static Filter _filter = Filter.Default();

        private SequenceModel(List<List<(FunctionCall, FunctionCall)>> sequence)
        {
            SequenceVariations = sequence;
        }

        static List<(FunctionCall, FunctionCall)> GetOrCreateSequence(ulong threadId)
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

        public static SequenceModel FromEventStream(IEnumerable<ProfilerEvent> eventStream)
        {
            var sequenceVariations = new List<List<(FunctionCall, FunctionCall)>>();
           Clear();

         
            foreach (var entry in eventStream)
            {
                if (entry.Token == Tokens.TokenEnter)
                {
                    var enterFunc = GetEnteredFunction(entry);

                    var stack = FindStackByThreadId(entry.ThreadId);

                    bool isEntry = enterFunc.IsEntry;
                    if (stack == null && isEntry)
                    {
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
                            stack.Pop();

                            if (!stack.Any())
                            {
                                // Sequence is complete
                                sequenceVariations.Add(GetOrCreateSequence(entry.ThreadId));

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
                    _tidToStack.Remove(entry.ThreadId);
                    _threadIdToSequence.Remove(entry.ThreadId);

                    // TODO close all open methods(!)
                }
            }

            var model = new SequenceModel(sequenceVariations);
            Clear();
            return model;
        }


      

    }
}