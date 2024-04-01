using System;

namespace SujaySarma.Data.Azure.Tables.Attributes
{
    /// <summary>
    /// The base class that is inherited by all other of our attributes. Do not decorate anything 
    /// with this attribute!
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class AttributeBase : Attribute
    {
    }
}
