using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Launcher
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
        public bool IsHidden;
        public bool IsEntry { get; set; }
    }

    interface IParserProgress
    {
        void Progress(int percent, int numEvent);
    }


    internal class ProfileParser
    {
        private readonly IParserProgress _progress;

        public ProfileParser(IParserProgress progress = null)
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
                line = line.Trim();
                var index = line.IndexOf(' ');
                if (index < 0)
                {
                    continue;
                }

                var funcId = ulong.Parse(line.Substring(0, index));
                var funcName = line.Substring(index + 1);
                var hidden = filter.IsHidden(funcName);
                var entry = filter.IsEntry(funcName);
                dictionary.Add(funcId, new FunctionInfo { Id = funcId, Name = funcName , IsHidden = hidden, IsEntry = entry});

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
                            _progress.Progress(currentProgress, numEvent);
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