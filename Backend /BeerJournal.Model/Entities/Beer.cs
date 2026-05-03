namespace BeerJournal.Model.Entities;

public class Beer
{
    public int BeerId { get; set; }
    public string Name { get; set; } = "";
    public string? Brand { get; set; }
    public decimal? AlcoholPct { get; set; }
    public string? Category { get; set; }
}