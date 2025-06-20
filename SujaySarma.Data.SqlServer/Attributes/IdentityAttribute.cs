using System;
using System.ComponentModel.DataAnnotations;

namespace SujaySarma.Data.SqlServer.Attributes
{
    /// <summary>
    /// For integer valued table columns, sets the column as an IDENTITY column on the server side.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class IdentityAttribute : ValidationAttribute
    {
        /// <summary>
        /// The value of SEED.
        /// </summary>
        public uint Seed
        {
            get; set;

        } = 1;

        /// <summary>
        /// The value of INCREMENT.
        /// </summary>
        public uint Increment
        {
            get; set;

        } = 1;

        /// <summary>
        /// For integer valued table columns, sets the column as an IDENTITY column on the server side.
        /// </summary>
        public IdentityAttribute(uint seed = 1, uint increment = 1)
        {
            Seed = seed;
            Increment = increment;
        }
    }
}
