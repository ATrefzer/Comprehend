using System;
using System.Collections.Generic;
using System.IO;

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

        internal IEnumerable<ProfilerEvent> Parse(string indexFile, string eventFile)
        {
            var funcIdToName = ParseIndex(indexFile);

            // Function Ids may change!
            //var distinctNames = funcIdToName.Values.Distinct();
            var stream = ParseEventStream(eventFile, funcIdToName);

            return stream;
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