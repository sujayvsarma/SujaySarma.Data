using System;

namespace SujaySarma.Data.SqlServer;

// Implementation of: Special exception class that is thrown only by this class.
public partial class SqlContext
{
    
    /// <summary>
    /// Exception encountered by methods in the <see cref="SqlContext"/> class.
    /// </summary>
    public class SqlContextException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlContextException"/> class.
        /// </summary>
        public SqlContextException()
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlContextException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public SqlContextException(string message)
            : base(message)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlContextException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public SqlContextException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }


}
