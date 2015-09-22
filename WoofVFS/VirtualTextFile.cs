using System;
using System.IO;
using System.Text;

namespace Woof.VFS {
    
    /// <summary>
    /// Virtual text file to be stored in memory
    /// </summary>
    public class VirtualTextFile : VirtualFSItem {

        /// <summary>
        /// String content of the file
        /// </summary>
        private string _Content;
        /// <summary>
        /// Text encoding for the file (default UTF-8 w/o BOM)
        /// </summary>
        public Encoding Encoding { get; set; }
        /// <summary>
        /// File text R/W access
        /// </summary>
        public string Content {
            get {
                try {
                    Lock.AcquireReaderLock(LockTimeout);
                    return _Content;
                } finally {
                    if (Lock.IsReaderLockHeld) Lock.ReleaseReaderLock();
                }
            }
            set {
                try {
                    Lock.AcquireWriterLock(LockTimeout);
                    _Content = value;
                    Modified = DateTime.Now;
                } finally {
                    if (Lock.IsWriterLockHeld) Lock.ReleaseWriterLock();
                }
            }
        }
        /// <summary>
        /// File as memory stream
        /// </summary>
        public MemoryStream Stream {
            get {
                if (Content != null) return new MemoryStream(Encoding.GetBytes(Content));
                else return null;
            }
        }
        /// <summary>
        /// Creates new virtual text file
        /// </summary>
        /// <param name="name"></param>
        /// <param name="content"></param>
        /// <param name="parent"></param>
        public VirtualTextFile(string name, string content = "", VirtualDirectory parent = null) {
            Encoding = Encoding.UTF8;
            Parent = parent;
            Name = name;
            Content = content;
        }
        /// <summary>
        /// Returns file name
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return Name;
        }

    }

}