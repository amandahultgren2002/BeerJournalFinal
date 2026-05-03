using BeerJournal.Model.Entities;
using BeerJournal.Model.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BeerJournal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TastingEntriesController : ControllerBase
{
    private readonly TastingEntryRepository _entryRepository;

    public TastingEntriesController(TastingEntryRepository entryRepository)
    {
        _entryRepository = entryRepository;
    }

    // GET api/TastingEntries
    // Returns only the logged-in user's entries
    [HttpGet]
    public IActionResult GetAll()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        return Ok(_entryRepository.GetEntriesByUser(userId.Value));
    }

    // GET api/TastingEntries/{id}
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var entry = _entryRepository.GetEntryById(id);

        if (entry == null) return NotFound();

        // Make sure users can only read their own entries
        if (entry.UserId != userId.Value) return Forbid();

        return Ok(entry);
    }

    // POST api/TastingEntries
    [HttpPost]
    public IActionResult Create([FromBody] TastingEntry entry)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        // Set userId from token — never trust the client for this
        entry.UserId = userId.Value;

        var result = _entryRepository.CreateEntry(entry);

        if (!result) return BadRequest();

        return Ok(new { message = "Entry created" });
    }

    // PUT api/TastingEntries/{id}
    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] TastingEntry entry)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var existing = _entryRepository.GetEntryById(id);
        if (existing == null) return NotFound();

        // Make sure users can only update their own entries
        if (existing.UserId != userId.Value) return Forbid();

        entry.EntryId = id;
        entry.UserId  = userId.Value;

        var result = _entryRepository.UpdateEntry(entry);

        if (!result) return NotFound();

        return Ok(new { message = "Entry created" });
    }

    // DELETE api/TastingEntries/{id}
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var existing = _entryRepository.GetEntryById(id);
        if (existing == null) return NotFound();

        // Make sure users can only delete their own entries
        if (existing.UserId != userId.Value) return Forbid();

        var result = _entryRepository.DeleteEntry(id);

        if (!result) return NotFound();

        return Ok(new { message = "Entry deleted" });
    }

    // Helper — reads userId from the JWT token claims
    private int? GetUserId()
    {
        var claim = User.FindFirst("userId")?.Value;
        if (claim == null) return null;
        return int.Parse(claim);
    }
}
