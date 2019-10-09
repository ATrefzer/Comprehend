using System.IO;

namespace Launcher.Profiler
{
    internal class Profile
    {
        private readonly string _directory;

        public Profile(string directory, string baseFile)
        {
            _directory = directory;
            BaseFile = baseFile;
        }

        public string BaseFile { get; }

        public string IndexFile => Path.Combine(_directory, BaseFile + ".index");
        public string EventFile => Path.Combine(_directory, BaseFile + ".profile");

        public override string ToString()
        {
            return BaseFile;
        }
    }
}