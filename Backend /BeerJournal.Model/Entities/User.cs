namespace BeerJournal.Model.Entities;

public class User
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public int? ZipCode { get; set; }
    public string? City { get; set; }
}