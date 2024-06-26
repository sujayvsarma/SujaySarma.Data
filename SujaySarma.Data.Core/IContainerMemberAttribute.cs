﻿using System;

using SujaySarma.Data.Core.Constants;

namespace SujaySarma.Data.Core
{
    /// <summary>
    /// An interface to be implemented by all attributes seeking to define a container's member (eg: Table Column) 
    /// for the class/structure/record property or fields they are decorated on.
    /// </summary>
    public interface IContainerMemberAttribute
    {
        /// <summary>
        /// The name of the underlying column (eg: name of the table column).
        /// </summary>
        string Name 
        { 
            get; init; 
        }

        /// <summary>
        /// Controls the inclusion of a column/field in data-modification 
        /// (insert, update and delete) operations.
        /// </summary>
        DataModificationInclusionBehaviour IncludeInDataModificationOperation 
        { 
            get; set; 
        }

        /// <summary>
        /// When the value of the property or field is an <seealso cref="System.Enum" /> type, this value controls 
        /// how that column is serialised.
        /// </summary>
        EnumSerializationBehaviour EnumSerializationStrategy 
        { 
            get; set; 
        }

        /// <summary>
        /// When set, this column shall act as a key in the WHERE clause. 
        /// </summary>
        bool IsSearchKey
        {
            get; set;
        }


        /// <summary>
        /// When set on a complex type or a type where we have no data-conversion implementation, 
        /// allows the property or field value use Json for storage and retrieval. Setting this value 
        /// to TRUE makes sense only if the underlying column/field is a <seealso cref="string"/> type.
        /// </summary>
        bool AllowSerializationAsJson 
        { 
            get; set; 
        }

        /// <summary>
        /// This function is called to retrieve the usable name for the container-member. Implementing attributes can use it 
        /// to apply prefixes, suffixes or even contextually modify the value of the <see cref="Name"/> property to 
        /// provide a different or better name for the operation.
        /// </summary>
        /// <returns>The qualified or usable name to use for the container-member</returns>
        string CreateQualifiedName();

        /// <summary>
        /// This function will be called to retrieve a value for the property or field when one is not set on it. The implementing 
        /// attribute may provide a usable value in such a case. This can be used to provide an "Autogenerated" value for fields 
        /// that are meant to be "keys" of some nature. 
        /// </summary>
        Func<object>? DefaultValueProviderFunction
        {
            get; set; 
        }
    }
}
