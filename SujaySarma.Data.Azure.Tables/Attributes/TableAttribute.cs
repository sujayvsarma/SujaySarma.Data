using System;

using SujaySarma.Data.Core;

namespace SujaySarma.Data.Azure.Tables.Attributes
{
    /// <summary>
    /// Provide name of the table the data for the class is stored in.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public sealed class TableAttribute : ContainerAttribute
    {
        /// <summary>
        /// Provides information about the table used to contain the data for an object.
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        public TableAttribute(string tableName)
            : base(tableName, false)
        {
        }

    }
}
