using System;

using SujaySarma.Data.Core;

namespace SujaySarma.Data.SqlServer.Attributes
{
    /// <summary>
    /// The base class that is inherited by all other of our attributes that define various properties of a table column. 
    /// DO NOT decorate anything with this attribute!
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class TableColumnPropertiesAttributeBase : ContainerMemberAttribute
    {

        /// <summary>
        /// Initialise
        /// </summary>
        /// <param name="name">Column name</param>
        protected TableColumnPropertiesAttributeBase(string name)
            : base(name)
        {
        }
    }
}
