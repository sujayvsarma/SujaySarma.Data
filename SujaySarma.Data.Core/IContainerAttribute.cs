namespace SujaySarma.Data.Core
{
    /// <summary>
    /// An interface to be implemented by all attributes seeking to define the "container" (eg: Tables)
    /// for the classes, structures or records they are decorated on.
    /// </summary>
    public interface IContainerAttribute
    {
        /// <summary>
        /// The name of the underlying container (eg: name of the table in a database).
        /// </summary>
        string Name { get; init; }

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
        bool UseSoftDelete { get; set; }

        /// <summary>
        /// This function is called to retrieve the usable name for the container. Implementing attributes can use it
        /// to apply prefixes, suffixes or even contextually modify the value of the <see cref="Name" /> property to
        /// provide a different or better name for the operation.
        /// </summary>
        /// <returns>The qualified or usable name to use for the container</returns>
        string CreateQualifiedName();
    }
}
