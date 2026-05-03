namespace BeerJournal.Model.Entities;

public class TastingEntry
{
    public int EntryId { get; set; }
    public int UserId { get; set; }

    // Foreign key to the beers table — replaces the four old fields
    // (BeerName, Brand, AlcoholPct, Category)
    public int BeerId { get; set; }

    // Tasting info — these stay on the entry
    public int? Rating { get; set; }
    public string? Location { get; set; }
    public DateOnly? TestingDate { get; set; }
    public decimal? Price { get; set; }
    public string? Notes { get; set; }

    // Map coordinates
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    // Nested beer object — populated when reading entries (via JOIN)
    // The frontend uses this to display beer name/brand/etc on each card
    // Nullable because on POST/PUT the client only sends BeerId, not the full beer
    public Beer? Beer { get; set; }
}