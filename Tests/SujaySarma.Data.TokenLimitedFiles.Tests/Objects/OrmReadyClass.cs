using System;

using SujaySarma.Data.Core.Attributes;
using SujaySarma.Data.TokenLimitedFiles.Attributes;

namespace SujaySarma.Data.TokenLimitedFiles.Tests.Objects;

/// <summary />
[Flatfile(headerRowIndex: 1)]
public class OrmReadyClass
{

    /// <summary />
    [FlatfileNamedField("Id", 1)]
    [OrmPopulatedGuidField(OrmPopulatedGuidField.ActivateOn.Empty)]
    protected Guid Id
    {
        get; set;
    }

    /// <summary />
    [FlatfileNamedField("Name", 2)]
    public string Name
    {
        get; set;
    }

    /// <summary />
    [FlatfileNamedField("LastModified", 3)]
    [OrmPopulatedTimestampField(DateTimeKind.Utc, zeroOutTime: false)]
    public DateTime LastModified
    {
        get; set;
    }

    /// <summary />
    [FlatfileNamedField("InternalField", 4)]
    public int _internalField = 99;

    /// <summary />
    public OrmReadyClass()
    {
        Name = "Sujay Sarma";
    }
}


/// <summary />
[Flatfile(headerRowIndex: 1)]
public class OrmUnreadyClass
{

    /// <summary />
    [FlatfileNamedField("Id", 1)]
    [OrmPopulatedGuidField(OrmPopulatedGuidField.ActivateOn.Empty)]
    protected Guid Id
    {
        get; set;
    }

    /// <summary />
    [FlatfileNamedField("Name", 2)]
    public string Name
    {
        get; set;
    }

    /// <summary />
    [FlatfileNamedField("LastModified", 2)]
    [OrmPopulatedTimestampField(DateTimeKind.Utc, zeroOutTime: false)]
    public DateTime LastModified
    {
        get; set;
    }

    /// <summary />
    [FlatfileNamedField("InternalField", 4)]
    public int _internalField = 99;

    /// <summary />
    public OrmUnreadyClass()
    {
        Name = "Sujay Sarma";
    }
}


/// <summary />
[Flatfile(headerRowIndex: 1)]
public class OrmReadyClassWithGaps
{

    /// <summary />
    [FlatfileNamedField("Id", 1)]
    [OrmPopulatedGuidField(OrmPopulatedGuidField.ActivateOn.Empty)]
    protected Guid Id
    {
        get; set;
    }

    /// <summary />
    [FlatfileNamedField("Name", 2)]
    public string Name
    {
        get; set;
    }

    /// <summary />
    [FlatfileNamedField("LastModified", 5)]
    [OrmPopulatedTimestampField(DateTimeKind.Utc, zeroOutTime: false)]
    public DateTime LastModified
    {
        get; set;
    }

    /// <summary />
    [FlatfileNamedField("InternalField", 8)]
    public int _internalField = 99;

    /// <summary />
    public OrmReadyClassWithGaps()
    {
        Name = "Sujay Sarma";
    }
}