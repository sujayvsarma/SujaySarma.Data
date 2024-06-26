﻿using System;

using SujaySarma.Data.Core.Reflection;

namespace SujaySarma.Data.SqlServer.Fluid.Tools
{
    /// <summary>
    /// Map between CLR object, Type discovery and the user provided table's alias
    /// </summary>
    internal class TypeTableAliasMap
    {
        /// <summary>
        /// If set, this is the table to be used in the FROM clause
        /// </summary>
        public bool IsPrimaryTable { get; set; } = false;

        /// <summary>
        /// Type of the CLR object we are storing a map of
        /// </summary>
        public Type ClrObjectType { get; set; } = default!;

        /// <summary>
        /// Type discovery information
        /// </summary>
        public ContainerTypeInformation Discovery { get; set; } = default!;

        /// <summary>
        /// User provided alias, if present
        /// </summary>
        public string Alias { get; set; } = default!;

        /// <summary>
        /// Returns if the provided table identifier is the same as the one registered for this object
        /// </summary>
        /// <param name="tableName">Table name to check</param>
        /// <returns>True/false</returns>
        public bool IsTable(string tableName)
            => (tableName.Equals(GetQualifiedTableName(), StringComparison.OrdinalIgnoreCase) ? true : false);

        /// <summary>
        /// Get the full table name including the schema
        /// </summary>
        /// <returns>Full table name including schema, always enclosed in []</returns>
        public string GetQualifiedTableName()
            => Discovery.ContainerDefinition.CreateQualifiedName();

    }
}
