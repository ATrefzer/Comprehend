using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Launcher
{
    internal class ProfilerEvent
    {
        public Tokens Token { get; set; }
        public ulong FunctioId { get; set; }
        public string FunctionName { get; set; }
        public ulong ThreadId { get; set; }
    }

    internal enum Tokens
    {
        TokenCreateThread,
        TokenDestroyThread,
        TokenEnter,
        TokenLeave,
        TokenTailCall
    }


    internal class ProfileParser
    {
        public InvocationModel CreateInvokationModel(IEnumerable<ProfilerEvent> stream)
        {
            var tidToStack = new Dictionary<ulong, Stack<FunctionCall>>();
            var functions = new Dictionary<string, FunctionCall>();

            foreach (var entry in stream)
            {
                if (entry.Token == Tokens.TokenEnter)
                {
                    // 1. Note as child of the active function
                    // 2. Push as active function

                    FunctionCall newFunc = null;
                    if (!functions.TryGetValue(entry.FunctionName, out newFunc))
                    {
                        newFunc = new FunctionCall();
                        newFunc.Id = entry.FunctioId;
                        newFunc.Name = entry.FunctionName;
                        functions.Add(newFunc.Name, newFunc);
                    }

                    // Find the correct thread such that we find the correct parent functiuons.
                    Stack<FunctionCall> stack;
                    if (!tidToStack.TryGetValue(entry.ThreadId, out stack))
                    {
                        stack = new Stack<FunctionCall>();
                        tidToStack.Add(entry.ThreadId, stack);
                    }

                    // Find active function
                    FunctionCall activeFunc = null;
                    if (stack.Count > 0)
                    {
                        activeFunc = stack.Peek();
                    }

                    if (activeFunc == newFunc) // reference
                    {
                        activeFunc.Recursive = true;
                    }

                    if (activeFunc != null)
                    {
                        activeFunc.Children.Add(newFunc);
                    }

                    // This is the new active function 
                    // Stack tracks recursive calls. But they do not appear in the model later.
                    stack.Push(newFunc);
                }
                else if (entry.Token == Tokens.TokenLeave)
                {
                    // Find the correct thread such that we find the correct parent functiuons.
                    Stack<FunctionCall> stack;
                    if (!tidToStack.TryGetValue(entry.ThreadId, out stack))
                    {
                        stack = new Stack<FunctionCall>();
                        tidToStack.Add(entry.ThreadId, stack);
                    }

                    // Find active function
                    FunctionCall activeFunc = null;
                    if (stack.Count > 0)
                    {
                        activeFunc = stack.Peek();
                    }

                    if (activeFunc != null && activeFunc.Name == entry.FunctionName)
                    {
                        stack.Pop();
                    }
                    else
                    {
                        // Ignore. We did not start recording at the time.
                    }
                }
                else if (entry.Token == Tokens.TokenTailCall)
                {
                    FunctionCall newFunc = null;
                    if (!functions.TryGetValue(entry.FunctionName, out newFunc))
                    {
                        newFunc = new FunctionCall();
                        newFunc.Id = entry.FunctioId;
                        newFunc.Name = entry.FunctionName;
                        functions.Add(newFunc.Name, newFunc);
                    }

                    newFunc.TailCall = true;
                }

                // TODO other events = changing thread ids!
            }

            return new InvocationModel(functions.Values.ToList());
        }

        internal IEnumerable<ProfilerEvent> Parse()
        {
            var funcIdToName = ParseIndex("d:\\index.txt");

            // Function Ids may change!
            //var distinctNames = funcIdToName.Values.Distinct();
            var stream = ParseEventStream("d:\\output.bin", funcIdToName);

            return stream;
        }

        private bool IsHidden(string functionName)
        {
            if (functionName.StartsWith("mscorlib"))
            {
                return true;
            }

            return false;
        }

        public Dictionary<ulong, string> ParseIndex(string path)
        {
            var dictionary = new Dictionary<ulong, string>();

            var reader = new StreamReader(path);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                var parts = line.Split(' ');
                if (parts.Length != 2)
                {
                    continue;
                }

                var funcId = ulong.Parse(parts[0]);
                var funcName = parts[1];

               
                    dictionary.Add(funcId, funcName);
            }

            return dictionary;
        }


        private IEnumerable<ProfilerEvent> ParseEventStream(string path, Dictionary<ulong, string> dictionary)
        {
            using (var stream = new FileStream(path, FileMode.Open))
            {
                var offset = 0;
                var bytes = new byte[10];

                while (stream.Read(bytes, 0, 10) == 10)
                {
                    // Token and thread id
                    offset += sizeof(ulong) + sizeof(ushort);

                    Tokens token;
                    ulong tid;
                    ulong fid;

                    token = (Tokens) BitConverter.ToUInt16(bytes, 0);
                    tid = BitConverter.ToUInt64(bytes, 2);

                    var entry = new ProfilerEvent();
                    entry.Token = token;
                    entry.ThreadId = tid;

                    if (token == Tokens.TokenEnter || token == Tokens.TokenLeave || token == Tokens.TokenTailCall)
                    {
                        stream.Read(bytes, 0, sizeof(ulong));
                        offset += sizeof(ulong);

                        fid = BitConverter.ToUInt64(bytes, 0);

                        //Debug.Assert(dictionary.ContainsKey(fid));
                        if (dictionary.ContainsKey(fid))
                        {
                            var funcName = dictionary[fid];
                            entry.FunctionName = funcName;
                        }
                        else
                        {
                            // Hidden
                            entry.FunctionName = "Unknown_" + fid;
                        }

                        entry.FunctioId = fid;
                    }

                    yield return entry;
                }
            }
        }
    }
}