using System;

namespace SujaySarma.Data.Core
{
    /// <summary>
    /// Define a data container, for example a table in a database.
    /// </summary>
    /// <remarks>
    ///     This is a simple implementation of the <see cref="IContainerAttribute"/> interface and can be used by all 
    ///     data storage and retrieval libraries when they need nothing more than the name of the container.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public class ContainerAttribute : Attribute, IContainerAttribute
    {
        /// <summary>
        /// The name of the underlying container (eg: name of the table in a database).
        /// </summary>
        public string Name 
        { 
            get; init; 
        }


        /// <summary>
        /// When set on the attribute, the underlying container supports the concept of soft-deletion. 
        /// </summary>
        /// <remarks>
        ///     When set on a member property or field, the corresponding library shall automatically add 
        ///     a Boolean-valued column named 'IsDeleted' to the underlying container (table). For rows that 
        ///     are meant as deleted, this column or field will be set to TRUE. Any subsequent query for data 
        ///     from that container shall not yield this row, even when the calling code explicitly queries 
        ///     for deleted rows, making it consistent with rows that are actually (hard) deleted from the container.
        /// </remarks>
        public bool UseSoftDelete 
        { 
            get; set; 
        }


        /// <summary>
        /// This function is called to retrieve the usable name for the container. Implementing attributes can use it 
        /// to apply prefixes, suffixes or even contextually modify the value of the <see cref="Name"/> property to 
        /// provide a different or better name for the operation.
        /// </summary>
        /// <returns>The qualified or usable name to use for the container</returns>
        public virtual string CreateQualifiedName()
            => Name;

        /// <summary>
        /// Defines a data container
        /// </summary>
        /// <param name="name">The name of the underlying container (eg: name of the table in a database).</param>
        /// <param name="useSoftDelete">When set on the attribute, the underlying container supports the concept of soft-deletion. </param>
        public ContainerAttribute(string name, bool useSoftDelete = false)
            : base()
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name), "Argument cannot be a Null, empty or whitespace string.");
            }

            Name = name;
            UseSoftDelete = useSoftDelete;
        }
    }
}
