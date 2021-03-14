using Microsoft.Extensions.FileProviders;
using System;
using System.IO;

namespace Vecc.GhostTemplating.RazorSupport
{
    public class FileInfo : IFileInfo
    {
        private readonly System.IO.FileInfo _file;

        public FileInfo(System.IO.FileInfo file)
        {
            _file = file;
        }

        public bool Exists => _file.Exists;

        public bool IsDirectory => false;

        public DateTimeOffset LastModified => new DateTimeOffset(_file.LastWriteTimeUtc);

        public long Length => _file.Length;

        public string Name => _file.Name;

        public string PhysicalPath => _file.FullName;

        public Stream CreateReadStream() => new StreamReader(_file.FullName).BaseStream;
    }
}
