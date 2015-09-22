using System;
using System.Threading;

namespace Woof.VFS {
    
    /// <summary>
    /// Virtual File System Item class
    /// </summary>
    public abstract class VirtualFSItem {

        /// <summary>
        /// Internal name
        /// </summary>
        private string _Name;
        /// <summary>
        /// Access lock for thread safe access
        /// </summary>
        protected ReaderWriterLock Lock = new ReaderWriterLock();
        /// <summary>
        /// Limit of the time to wait for concurent operations to release the lock
        /// </summary>
        protected int LockTimeout = 1000;
        /// <summary>
        /// Parent directory of current item (null means this is root)
        /// </summary>
        public VirtualDirectory Parent { get; protected set; }
        /// <summary>
        /// Item name
        /// </summary>
        public string Name {
            get { return _Name; }
            set { _Name = value; Modified = DateTime.Now; }
        }
        /// <summary>
        /// Item creation time
        /// </summary>
        public DateTime Created { get; set; }
        /// <summary>
        /// Item modification time
        /// </summary>
        public DateTime Modified { get; set; }

    }

}