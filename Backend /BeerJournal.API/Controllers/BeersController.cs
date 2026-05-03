using BeerJournal.Model.Entities;
using BeerJournal.Model.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace BeerJournal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BeersController : ControllerBase
{
    private readonly BeerRepository _beerRepository;

    public BeersController(BeerRepository beerRepository)
    {
        _beerRepository = beerRepository;
    }

    // GET api/Beers
    // Returns the full catalogue — used by the dropdown in log-beer
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_beerRepository.GetAllBeers());
    }

    // GET api/Beers/{id}
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var beer = _beerRepository.GetBeerById(id);
        if (beer == null) return NotFound();
        return Ok(beer);
    }

    // POST api/Beers
    // Creates a new beer in the shared catalogue
    [HttpPost]
    public IActionResult Create([FromBody] Beer beer)
    {
        if (string.IsNullOrWhiteSpace(beer.Name))
            return BadRequest("Beer name is required");

        var created = _beerRepository.CreateBeer(beer);

        if (created == null) return BadRequest();

        return Ok(created);
    }

    // PUT api/Beers/{id}
    // Updates an existing beer in the catalogue
    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] Beer beer)
    {
        if (string.IsNullOrWhiteSpace(beer.Name))
            return BadRequest("Beer name is required");

        // Make sure the beer exists before we try to update
        var existing = _beerRepository.GetBeerById(id);
        if (existing == null) return NotFound();

        // Force the id from the URL — never trust the client to set it in the body
        beer.BeerId = id;

        var result = _beerRepository.UpdateBeer(beer);

        if (!result) return NotFound();

        return Ok(new { message = "Beer updated" });
    }

    // DELETE api/Beers/{id}
    // Will fail if any tasting entries still reference this beer
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var existing = _beerRepository.GetBeerById(id);
        if (existing == null) return NotFound();

        try
        {
            var result = _beerRepository.DeleteBeer(id);
            if (!result) return NotFound();

            return Ok(new { message = "Beer deleted" });
        }
        catch (PostgresException ex) when (ex.SqlState == "23503")
        {
            // 23503 = foreign_key_violation
            // Means tasting_entries still reference this beer
            return Conflict(new
            {
                message = "Cannot delete this beer — it is referenced by tasting entries"
            });
        }
    }
}