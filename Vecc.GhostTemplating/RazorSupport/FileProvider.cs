using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System.IO;

namespace Vecc.GhostTemplating.RazorSupport
{
    public class FileProvider : IFileProvider
    {
        private readonly string _root;

        public FileProvider()
        {
            //TODO: how to get this?
            _root = ".";
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            var result = new DirectoryContents(System.IO.Path.Combine(_root, subpath.TrimStart('/').TrimStart('\\')));

            return result;
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            var file = new System.IO.FileInfo(Path.Combine(_root, subpath.TrimStart('/').TrimStart('\\')));
            var result = new FileInfo(file);

            return result;
        }

        public IChangeToken Watch(string filter)
        {
            return null;
        }
    }
}
