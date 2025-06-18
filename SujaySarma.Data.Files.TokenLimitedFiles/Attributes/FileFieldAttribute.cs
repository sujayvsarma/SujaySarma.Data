using SujaySarma.Data.Core;

using System;

namespace SujaySarma.Data.Files.TokenLimitedFiles.Attributes
{
    /// <summary>
    /// Provide metadata about the field/column in the delimited file
    /// </summary>
    /// <remarks>
    ///     The Name property is not nullable and you cannot set an empty string into it. If the underlying flatfile does not
    ///     contain column headers, set the Name property to any dummy string and use the AddressingPreference property to
    ///     pick Indices as the method.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class FileFieldAttribute : ContainerMemberAttribute
    {
        /// <summary>
        /// Zero-based index of the column in the flatfile for this element.
        /// Set to any negative value to ignore this parameter
        /// </summary>
        public int ColumnIndex { get; set; } = -1;

        /// <summary>
        /// Strategy to use when identifying what a flatfile column is during data read or write operations.
        /// </summary>
        public FieldAddressingStrategy AddressingPreference { get; set; }

        /// <summary>
        /// Only valid when the underlying .NET type is a Boolean.
        /// Collection of values from the flatfile to treat as "true"
        /// </summary>
        public string[] TrueValues { get; set; } = WellknownValuesCollections.TrueValues.ToArray();

        /// <summary>
        /// Only valid when the underlying .NET type is a Boolean.
        /// Collection of values from the flatfile to treat as "false"
        /// </summary>
        public string[] FalseValues { get; set; } = WellknownValuesCollections.FalseValues.ToArray();

        /// <summary>
        /// Returns the appropriate name to use for this field in the flatfile
        /// </summary>
        /// <returns>String</returns>
        public override string CreateQualifiedName()
        {
            FieldAddressingStrategy addressingPreference = this.AddressingPreference;
            string qualifiedName = string.Empty;
            switch (addressingPreference)
            {
                case FieldAddressingStrategy.Name:
                    qualifiedName = Name;
                    break;

                case FieldAddressingStrategy.Indices:
                    qualifiedName = $"Column {ColumnIndex}";
                    break;
            }
            return qualifiedName;
        }

        /// <summary>
        /// Provide metadata about the field/column in the delimited file
        /// </summary>
        /// <param name="name">Name of the flatfile column. Use a dummy value if column or flatfile does not have a column-name</param>
        public FileFieldAttribute(string name)
          : base(name)
        {
            AddressingPreference = FieldAddressingStrategy.Name;
        }

        /// <summary>
        /// Provide metadata about the field/column in the delimited file
        /// </summary>
        /// <param name="index">Zero-based index of the column in the flatfile</param>
        public FileFieldAttribute(int index)
          : base(Guid.NewGuid().ToString("n"))
        {
            ColumnIndex = index;
            AddressingPreference = FieldAddressingStrategy.Indices;
        }

        /// <summary>
        /// Provide metadata about the field/column in the delimited file
        /// </summary>
        /// <param name="name">Name of the flatfile column. Use a dummy value if column or flatfile does not have a column-name</param>
        /// <param name="index">Zero-based index of the column in the flatfile</param>
        /// <param name="preference">Preference of name or index while reading or writing values</param>
        public FileFieldAttribute(string name, int index, FieldAddressingStrategy preference)
          : base(name)
        {
            ColumnIndex = index;
            AddressingPreference = preference;
        }
    }

}
