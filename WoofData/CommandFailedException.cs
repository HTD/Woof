using System;
using System.Data.SqlClient;

namespace Woof.Data {

    /// <summary>
    /// Raised with severe SQL errors over 10 where FireInfoMessageEventOnUserErrors is true
    /// </summary>
    public class CommandFailedException : ApplicationException {
        public SqlErrorCollection Errors { get; set; }
        public CommandFailedException() : base() { }
        public CommandFailedException(string message) : base(message) { }
        public CommandFailedException(SqlErrorCollection e) : base(e[0].Message) { Errors = e; }
    }

}