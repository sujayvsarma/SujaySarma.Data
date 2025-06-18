using System;

namespace SujaySarma.Data.Core
{
    /// <summary>
    /// Define a data container, for example a table in a database.
    /// </summary>
    /// <remarks>
    ///     This is a simple implementation of the <see cref="IContainerAttribute" /> interface and can be used by all
    ///     data storage and retrieval libraries when they need nothing more than the name of the container.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public class ContainerAttribute : Attribute, IContainerAttribute
    {
        /// <summary>
        /// The name of the underlying container (eg: name of the table in a database).
        /// </summary>
        public string Name { get; init; } = default!;

        /// <summary>
        /// When set on the attribute, the underlying container supports the concept of soft-deletion.
        /// </summary>
        public bool UseSoftDelete { get; set; } = false;

        /// <summary>
        /// This function is called to retrieve the usable name for the container.
        /// </summary>
        /// <returns>The qualified or usable name to use for the container</returns>
        public virtual string CreateQualifiedName() => Name;

        /// <summary>Defines a data container</summary>
        /// <param name="name">The name of the underlying container (eg: name of the table in a database).</param>
        /// <param name="useSoftDelete">When set on the attribute, the underlying container supports the concept of soft-deletion. </param>
        public ContainerAttribute(string name, bool useSoftDelete = false)
        {
            Name = ((!string.IsNullOrWhiteSpace(name)) ? name : throw new ArgumentNullException(nameof(name), "Argument cannot be a Null, empty or whitespace string."));
            UseSoftDelete = useSoftDelete;
        }
    }
}
