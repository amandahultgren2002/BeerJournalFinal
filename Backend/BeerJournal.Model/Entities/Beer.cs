namespace BeerJournal.Model.Entities;

// Beer entity — represents one row in the beers table

public class Beer
{
    public int BeerId { get; set; }
    public string Name { get; set; } = "";
    public string? Brand { get; set; }
    public decimal? AlcoholPct { get; set; }
    public string? Category { get; set; }
}