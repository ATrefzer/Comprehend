using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    class ProfilerEvent
    {
        public Tokens Token { get; set; }
        public ulong FunctioId { get; set; }
        public string FunctionName { get; set; }
        public ulong ThreadId { get; set; }
    }

    enum Tokens
    {
        TokenCreateThread,
        TokenDestroyThread,
        TokenEnter,
        TokenLeave,
        TokenTailCall,
    };



    class ProfileParser
    {
        Dictionary<ulong, string> ParseIndex(string path)
        {
            var dictionary = new Dictionary<ulong, string>();
            var lines = File.ReadAllLines(path);
            foreach (var line in lines)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    var parts = line.Split(' ');
                    if (parts.Length != 2) continue;

                    var funcId = ulong.Parse(parts[0]);
                    var funcName = parts[1];

                    dictionary.Add(funcId, funcName);
                }
            }
            return dictionary;
        }

        internal List<ProfilerEvent> Parse()
        {
            var funcIdToName = ParseIndex("d:\\index.txt");
            var stream = ParseEventStream("d:\\output.bin", funcIdToName);

            return stream;
        }

        private List<ProfilerEvent> ParseEventStream(string path, Dictionary<ulong, string> dictionary)
        {
            List<ProfilerEvent> eventStream = new List<ProfilerEvent>();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                var offset = 0;
                byte[] bytes = new byte[10];

                while (stream.Read(bytes, 0, 10) == 10)
                {
                    // Token and thread id
                    offset += sizeof(ulong) + sizeof(ushort);

                    Tokens token;
                    ulong tid;
                    ulong fid;

                    token = (Tokens)BitConverter.ToUInt16(bytes, 0);
                    tid = BitConverter.ToUInt64(bytes, 2);

                    var entry = new ProfilerEvent();
                    entry.Token = token;
                    entry.ThreadId = tid;

                    if (token == Tokens.TokenEnter || token == Tokens.TokenLeave || token == Tokens.TokenTailCall)
                    {
                        stream.Read(bytes, 0, sizeof(ulong));
                        offset += sizeof(ulong);

                        fid = BitConverter.ToUInt64(bytes, 0);

                        var funcName = dictionary[fid];
                        entry.FunctioId = fid;
                        entry.FunctionName = funcName;
                    }
                    eventStream.Add(entry);
                }
                return eventStream;
            }
        }
    }
}
