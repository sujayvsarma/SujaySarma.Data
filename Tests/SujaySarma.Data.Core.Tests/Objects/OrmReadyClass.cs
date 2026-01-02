using System;
using System.ComponentModel.DataAnnotations.Schema;

using SujaySarma.Data.Core.Attributes;

namespace SujaySarma.Data.Core.Tests.Objects;

/// <summary />
[PersistenceContainer("TableName1")]
[Table("TableName1")]
public class OrmReadyClass
{

    /// <summary />
    [PersistenceContainerMember("Id")]
    [OrmPopulatedGuidField(OrmPopulatedGuidField.ActivateOn.Empty)]
    protected Guid Id
    {
        get; set;
    }

    /// <summary />
    [PersistenceContainerMember("Name")]
    public string Name
    {
        get; set;
    }

    /// <summary />
    [PersistenceContainerMember("LastModified")]
    [OrmPopulatedTimestampField(DateTimeKind.Utc, zeroOutTime: false)]
    public DateTime LastModified
    {
        get; set;
    }

    /// <summary />
    [PersistenceContainerMember("InternalField")]
    public int _internalField = 99;

    /// <summary />
    public OrmReadyClass()
    {
        Name = "Sujay Sarma";
    }
}
