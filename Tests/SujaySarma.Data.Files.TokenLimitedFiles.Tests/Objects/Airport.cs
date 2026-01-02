using SujaySarma.Data.Files.TokenLimitedFiles.Attributes;

namespace SujaySarma.Data.Files.TokenLimitedFiles.Tests.Objects;

/// <summary>
/// Represents an airport record from the perfbenchmark.csv file
/// </summary>
[Flatfile(1, TableName = "airports")]
public class Airport
{
    [FlatfileNamedField("id", 1)]
    public int Id { get; set; }

    [FlatfileNamedField("ident", 2)]
    public string Ident { get; set; } = string.Empty;

    [FlatfileNamedField("type", 3)]
    public string Type { get; set; } = string.Empty;

    [FlatfileNamedField("name", 4)]
    public string Name { get; set; } = string.Empty;

    [FlatfileNamedField("latitude_deg", 5)]
    public double? LatitudeDegrees { get; set; }

    [FlatfileNamedField("longitude_deg", 6)]
    public double? LongitudeDegrees { get; set; }

    [FlatfileNamedField("elevation_ft", 7)]
    public int? ElevationFeet { get; set; }

    [FlatfileNamedField("continent", 8)]
    public string Continent { get; set; } = string.Empty;

    [FlatfileNamedField("iso_country", 9)]
    public string IsoCountry { get; set; } = string.Empty;

    [FlatfileNamedField("iso_region", 10)]
    public string IsoRegion { get; set; } = string.Empty;

    [FlatfileNamedField("municipality", 11)]
    public string Municipality { get; set; } = string.Empty;

    [FlatfileNamedField("scheduled_service", 12)]
    public string ScheduledService { get; set; } = string.Empty;

    [FlatfileNamedField("icao_code", 13)]
    public string? IcaoCode { get; set; }

    [FlatfileNamedField("iata_code", 14)]
    public string? IataCode { get; set; }

    [FlatfileNamedField("gps_code", 15)]
    public string GpsCode { get; set; } = string.Empty;

    [FlatfileNamedField("local_code", 16)]
    public string LocalCode { get; set; } = string.Empty;

    [FlatfileNamedField("home_link", 17)]
    public string? HomeLink { get; set; }

    [FlatfileNamedField("wikipedia_link", 18)]
    public string? WikipediaLink { get; set; }

    [FlatfileNamedField("keywords", 19)]
    public string? Keywords { get; set; }
}