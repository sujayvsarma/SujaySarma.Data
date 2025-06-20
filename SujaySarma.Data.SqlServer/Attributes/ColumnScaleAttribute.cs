using System;
using System.ComponentModel.DataAnnotations;

namespace SujaySarma.Data.SqlServer.Attributes
{
    /// <summary>
    /// For numerical SQL Server columns that support it, provides the "scale".
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ColumnScaleAttribute : ValidationAttribute
    {
        /// <summary>
        /// The value of scale.
        /// </summary>
        public uint Scale
        {
            get; set;

        } = 0;

        /// <summary>
        /// For numerical SQL Server columns that support it, provides the "scale".
        /// </summary>
        public ColumnScaleAttribute(uint scale)
            => Scale = scale;
    }
}
