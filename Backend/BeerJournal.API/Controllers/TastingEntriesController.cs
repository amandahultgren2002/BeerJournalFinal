// TastingEntriesController — handles all CRUD operations for tasting entries
// Every endpoint requires the user to be logged in (JWT) thanks to [Authorize]

using BeerJournal.Model.Entities;
using BeerJournal.Model.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BeerJournal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]   // every endpoint below requires a valid JWT token
public class TastingEntriesController : ControllerBase
{
    private readonly TastingEntryRepository _entryRepository;

    public TastingEntriesController(TastingEntryRepository entryRepository)
    {
        _entryRepository = entryRepository;
    }

    // GET /api/TastingEntries — return only the logged-in user's entries
    [HttpGet]
    public IActionResult GetAll()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        return Ok(_entryRepository.GetEntriesByUser(userId.Value));
    }

    // GET /api/TastingEntries/{id} — return one entry by id
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

    // POST /api/TastingEntries — create a new entry
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

    // PUT /api/TastingEntries/{id} — update an existing entry
    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] TastingEntry entry)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var existing = _entryRepository.GetEntryById(id);
        if (existing == null) return NotFound();

        // Make sure users can only update their own entries
        if (existing.UserId != userId.Value) return Forbid();

        // Set the id and userId from trusted sources, not the request body
        entry.EntryId = id;
        entry.UserId  = userId.Value;

        var result = _entryRepository.UpdateEntry(entry);
        if (!result) return NotFound();

        return Ok(new { message = "Entry updated" });
    }

    // DELETE /api/TastingEntries/{id} — delete an entry
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