using Microsoft.Extensions.FileProviders;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Vecc.GhostTemplating.RazorSupport
{
    public class DirectoryContents : IDirectoryContents
    {
        private readonly DirectoryInfo _directory;

        public DirectoryContents(string path)
        {
            _directory = new DirectoryInfo(path);
        }

        public bool Exists => _directory.Exists;

        public IEnumerator<IFileInfo> GetEnumerator()
        {
            foreach (var file in _directory.GetFiles())
            {
                var item = new FileInfo(file);
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
