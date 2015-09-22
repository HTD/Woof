using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Woof.VFS {

    /// <summary>
    /// Container for virtual files and directories
    /// </summary>
    public class VirtualDirectory : VirtualFSItem {

        /// <summary>
        /// If true, file name search is case insensitive
        /// </summary>
        public bool IgnoreCase { get; set; }

        /// <summary>
        /// Virtual file system items
        /// </summary>
        public List<VirtualFSItem> Items = new List<VirtualFSItem>();

        /// <summary>
        /// Virtual file list
        /// </summary>
        public List<VirtualTextFile> Files { get { return Items.Where(i => i is VirtualTextFile).Select(i => i as VirtualTextFile).ToList(); } }

        /// <summary>
        /// Virtual directory list
        /// </summary>
        public List<VirtualDirectory> Dirs { get { return Items.Where(i => i is VirtualDirectory).Select(i => i as VirtualDirectory).ToList(); } }

        /// <summary>
        /// Returns virtual file with specified name or creates one if create is true and file doesn't exist
        /// </summary>
        /// <param name="name"></param>
        /// <param name="create"></param>
        /// <returns></returns>
        /// <exception cref="System.IO.FileNotFoundException"></exception>
        public VirtualTextFile File(string name, bool create = false) {
            try {
                Lock.AcquireReaderLock(LockTimeout);
                var item =
                    IgnoreCase
                        ? Items.FirstOrDefault(i => String.Equals(i.Name, name, StringComparison.CurrentCultureIgnoreCase) && i is VirtualTextFile) as VirtualTextFile
                        : Items.FirstOrDefault(i => i.Name == name && i is VirtualTextFile) as VirtualTextFile;
                if (item != null) return item;
                else {
                    if (create) {
                        Lock.UpgradeToWriterLock(LockTimeout);
                        return _CreateFile(name);
                    } else throw new FileNotFoundException("Virtual file not found", name);
                }
            } finally {
                if (Lock.IsReaderLockHeld) Lock.ReleaseReaderLock();
                if (Lock.IsWriterLockHeld) Lock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// Returns virtual directory with specified name or creates one if create is true and directory doesn't exist
        /// </summary>
        /// <param name="name"></param>
        /// <param name="create"></param>
        /// <returns></returns>
        /// <exception cref="System.IO.DirectoryNotFoundException"></exception>
        public VirtualDirectory Directory(string name, bool create = false) {
            try {
                var item = Items.FirstOrDefault(i => i.Name == name && i is VirtualDirectory) as VirtualDirectory;
                if (item != null) return item;
                else {
                    if (create) {
                        Lock.UpgradeToWriterLock(LockTimeout);
                        return _CreateDirectory(name);
                    } else throw new DirectoryNotFoundException("Virtual directory not found");
                }
            } finally {
                if (Lock.IsReaderLockHeld) Lock.ReleaseReaderLock();
                if (Lock.IsWriterLockHeld) Lock.ReleaseWriterLock();
            }

        }

        /// <summary>
        /// Clears all contained items (unsafe)
        /// </summary>
        private void _Clear() {
            Items.Clear();
        }

        /// <summary>
        /// Creates a virtual directory with specified name and returns it or retuns existing directory (unsafe)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private VirtualDirectory _CreateDirectory(string name) {
            var directory = Items.FirstOrDefault(i => i.Name == name && i is VirtualDirectory) as VirtualDirectory;
            if (directory == null) {
                Modified = DateTime.Now;
                directory = new VirtualDirectory(name, this);
                Items.Add(directory);
            }
            return directory;
        }

        /// <summary>
        /// Creates a virtual file with specified name and content and returns it or retuns existing file with changed content (unsafe)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        private VirtualTextFile _CreateFile(string name, string content = "") {
            var file = Items.FirstOrDefault(i => i.Name == name && i is VirtualTextFile) as VirtualTextFile;
            if (file == null) {
                Modified = DateTime.Now;
                file = new VirtualTextFile(name, content, this);
                Items.Add(file);
            }
            file.Content = content;
            return file;
        }

        /// <summary>
        /// Removes virtual directory if exists (unsafe)
        /// </summary>
        /// <param name="name"></param>
        private void _RemoveDirectory(string name) {
            var item = Items.FirstOrDefault(i => i.Name == name && i is VirtualDirectory);
            if (item != null) { Items.Remove(item); Modified = DateTime.Now; }
        }

        /// <summary>
        /// Removes virtual file if exists (unsafe)
        /// </summary>
        /// <param name="name"></param>
        private void _RemoveFile(string name) {
            var item = Items.FirstOrDefault(i => i.Name == name && i is VirtualTextFile);
            if (item != null) { Items.Remove(item); Modified = DateTime.Now; }
        }

        /// <summary>
        /// Clears all contained items
        /// </summary>
        public void Clear() {
            try {
                Lock.AcquireWriterLock(LockTimeout);
                _Clear();
            } finally {
                if (Lock.IsWriterLockHeld) Lock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// Creates a virtual directory with specified name and returns it or retuns existing directory
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public VirtualDirectory CreateDirectory(string name) {
            try {
                Lock.AcquireWriterLock(LockTimeout);
                return _CreateDirectory(name);
            } finally {
                if (Lock.IsWriterLockHeld) Lock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// Creates a virtual file with specified name and content and returns it or retuns existing file with changed content
        /// </summary>
        /// <param name="name"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public VirtualTextFile CreateFile(string name, string content) {
            try {
                Lock.AcquireWriterLock(LockTimeout);
                return _CreateFile(name, content);
            } finally {
                if (Lock.IsWriterLockHeld) Lock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// Returns true if virtual file with specified name exists
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool FileExists(string name) {
            try {
                Lock.AcquireReaderLock(LockTimeout);
                return
                    IgnoreCase
                        ? Items.Exists(i => String.Equals(i.Name, name, StringComparison.CurrentCultureIgnoreCase) && i is VirtualTextFile)
                        : Items.Exists(i => i.Name == name && i is VirtualTextFile);
            } finally {
                if (Lock.IsReaderLockHeld) Lock.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// Removes virtual directory if exists
        /// </summary>
        /// <param name="name"></param>
        public void RemoveDirectory(string name) {
            try {
                Lock.AcquireWriterLock(LockTimeout);
                _RemoveDirectory(name);
            } finally {
                if (Lock.IsWriterLockHeld) Lock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// Removes virtual file if exists
        /// </summary>
        /// <param name="name"></param>
        public void RemoveFile(string name) {
            try {
                Lock.AcquireWriterLock(LockTimeout);
                _RemoveFile(name);
            } finally {
                if (Lock.IsWriterLockHeld) Lock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// Returns new virtual file containing a list of all files contained in virtual directory with modification dates first
        /// </summary>
        /// <returns></returns>
        public VirtualTextFile ListFiles() {
            try {
                Lock.AcquireReaderLock(LockTimeout);
                var b = new StringBuilder();
                Items.ForEach(i => {
                    if (i is VirtualTextFile) {
                        b.AppendLine(string.Format("[{0}] {1}", i.Modified.ToString("yyyy-MM-dd HH:mm:ss"), i.Name));
                    }
                });
                return new VirtualTextFile("List", b.ToString());
            } finally {
                if (Lock.IsReaderLockHeld) Lock.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// Creates new virtual directory
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        public VirtualDirectory(string name = "/", VirtualDirectory parent = null) {
            Parent = parent;
            Name = name;
            Created = Modified = DateTime.Now;
        }

    }

}