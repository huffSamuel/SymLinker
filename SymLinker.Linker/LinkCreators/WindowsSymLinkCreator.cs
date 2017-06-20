using SymLinker.Linker;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SymLinker.Linker.LinkCreators
{
    internal class WindowsSymLinkCreator : ISymLinkCreator
    {
        [DllImport("kernel32.dll")]
        static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, SymbolicLink dwFlags);

        public bool CreateSymLink(string linkPath, string targetPath, bool file)
        {
            var linkName = Path.GetFileName(targetPath);
            linkPath = Path.Combine(linkPath, linkName);
            bool success = false;
            try
            {
                var symbolicLinkType = file ? SymbolicLink.File : SymbolicLink.Directory;
                success = CreateSymbolicLink(linkPath, targetPath, symbolicLinkType);
            }
            catch(Exception ex)
            {
                // TODO: Handle exception
            }
            return success;
        }

        private enum SymbolicLink
        {
            File = 0,
            Directory = 1
        }

    }
}
