using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using Launcher.Execution;

namespace Launcher.Profiler
{
    public class ProfilerEvent
    {
        public Tokens Token { get; set; }

        // Only valid for function calls!
        public FunctionInfo Func { get; set; }

        public ulong ThreadId { get; set; }
    }

    public enum Tokens
    {
        TokenCreateThread,
        TokenDestroyThread,
        TokenEnter,
        TokenLeave,
        TokenTailCall
    }


    internal class ProfileParser
    {
        private readonly IProgress _progress;

        public ProfileParser(IProgress progress = null)
        {
            _progress = progress;
        }

        /// <summary>
        /// Returns the dictionary with all functions. The functions are marked as "included" according to the filter.
        /// </summary>
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

                // Some method names contain spaces. Fixed spaces in profiler dll.

                line = line.Trim();
                var parts = line.Split(new[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                Debug.Assert(parts.Length == 3);

                var funcId = ulong.Parse(parts[0].Trim());
                var funcName = parts[1].Trim();
                var isPublic = parts[2] == "+";
                
                var filtered = filter.IsFiltered(funcName);
                dictionary.Add(funcId, new FunctionInfo(funcId, funcName, isPublic, filtered));

                // Note(!)
                // Same function is recorded multiple times with different ids!
            }

            return dictionary;
        }

        public IEnumerable<ProfilerEvent> ParseEventStream(string path, Dictionary<ulong, FunctionInfo> idToFuncInfo)
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
                        var currentProgress = (int) (stream.Position / (double) length * 100);
                        if (currentProgress >= lastProgress + 1)
                        {
                            var message = "Reading profile file: Event Nr: " + numEvent;
                            _progress.Progress(message, currentProgress);
                            lastProgress = currentProgress;
                        }
                    }

                    // Token and thread id
                    var token = (Tokens) reader.ReadUInt16();
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
                        Debug.Assert(idToFuncInfo.ContainsKey(fid));
                        entry.Func = idToFuncInfo[fid];
                    }

                    yield return entry;
                }
            }
        }
    }
}