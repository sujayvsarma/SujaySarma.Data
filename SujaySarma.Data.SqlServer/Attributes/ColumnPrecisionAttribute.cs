using System;
using System.ComponentModel.DataAnnotations;

namespace SujaySarma.Data.SqlServer.Attributes
{
    /// <summary>
    /// For numerical SQL Server columns that support it, provides the "precision".
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ColumnPrecisionAttribute : ValidationAttribute
    {
        /// <summary>
        /// The value of precision.
        /// </summary>
        public uint Precision
        {
            get; set;

        } = 18;

        /// <summary>
        /// For numerical SQL Server columns that support it, provides the "precision".
        /// </summary>
        public ColumnPrecisionAttribute(uint precision)
            => Precision = precision;
    }
}
