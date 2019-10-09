using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Policy;

using Launcher.Common;
using Launcher.Execution;

namespace Launcher.Profiler
{
    internal class ProfilerEvent
    {
        public Tokens Token { get; set; }

        // Only valid for function calls!
        public FunctionInfo Func { get; set; }

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

    public class FunctionInfo
    {
        public ulong Id;
        public string Name;
        public bool IsFiltered;
        public bool IsEntry { get; set; }
        public bool IsPublic { get; set; }
    }


    internal class ProfileParser
    {
        private readonly IProgress _progress;

        public ProfileParser(IProgress progress = null)
        {
            _progress = progress;
        }

        public Dictionary<ulong, FunctionInfo> ParseIndex(string path, Filter filter)
        {
            // Does a function name appear under different ids?
            var dictionary = new Dictionary<ulong, FunctionInfo>();

            var reader = new StreamReader(path);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                // Some method names contain spaces. We string.split does not work reliably.
                // Fixed spaces in profiler dll.


                line = line.Trim();
                var parts = line.Split(new []{'\t'}, StringSplitOptions.RemoveEmptyEntries);
                Debug.Assert((parts.Length == 3));
                

                var funcId = ulong.Parse(parts[0].Trim());
                var funcName = parts[1].Trim();
                var isPublic = parts[2] == "+" ? true : false;;
                var filtered = filter.IsFiltered(funcName);
                var entry = filter.IsEntry(funcName);
                dictionary.Add(funcId, new FunctionInfo { Id = funcId, Name = funcName , IsFiltered = filtered, IsEntry = entry, IsPublic = isPublic});

                // Note(!)
                // Same function is recorded multiple times with different ids!
            }

            return dictionary;
        }

        internal IEnumerable<ProfilerEvent> Parse(string indexFile, string eventFile, Filter filter)
        {
            var funcIdToName = ParseIndex(indexFile, filter);

            // Function Ids may change!
            //var distinctNames = funcIdToName.Values.Distinct();
            var stream = ParseEventStream(eventFile, funcIdToName);

            return stream;
        }

        private IEnumerable<ProfilerEvent> ParseEventStream(string path, Dictionary<ulong, FunctionInfo> dictionary)
        {
            var lastProgress = 0;
            using (var stream = new FileStream(path, FileMode.Open))
            {
                var length = stream.Length;

                var reader = new BinaryReader(stream);
                var numEvent = 0;

                while (stream.Position < length)
                {
                    // Progress
                    numEvent++;

                    if (_progress != null)
                    {
                        var currentProgress = (int) ((stream.Position / (double) length) * 100);
                        if (currentProgress >= lastProgress + 1)
                        {
                            string message = "Reading profile file: Event Nr: " + numEvent;
                            _progress.Progress(message, currentProgress);
                            lastProgress = currentProgress;
                        }
                    }

                    // Token and thread id
                    var token = (Tokens)reader.ReadUInt16();
                    var tid = reader.ReadUInt64();

                    var entry = new ProfilerEvent();
                    entry.Token = token;
                    entry.ThreadId = tid;

                    // Debug
                    //if (numEvent == 164062777-1)
                    //{
                    //    Debugger.Break();
                    //}

                    if (token == Tokens.TokenEnter || token == Tokens.TokenLeave || token == Tokens.TokenTailCall)
                    {
                        var fid = reader.ReadUInt64();
                        Debug.Assert(dictionary.ContainsKey(fid));
                        entry.Func = dictionary[fid];
                    }


                    yield return entry;
                }
            }
        }
    }
}