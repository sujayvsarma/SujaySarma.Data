using SujaySarma.Data.Core;

using System;

namespace SujaySarma.Data.Azure.Tables.Attributes
{
    /// <summary>
    /// Provides information about the table used to contain the data for an object.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public sealed class TableAttribute : ContainerAttribute
    {
        /// <summary>
        /// Provides information about the table used to contain the data for an object.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        public TableAttribute(string tableName)
            : base(tableName)
        {
        }


        /// <summary>
        /// Default constructor.
        /// Made PRIVATE so nobody can use it.
        /// </summary>
        private TableAttribute() 
            : base(string.Empty) 
        {
        }
    }
}
