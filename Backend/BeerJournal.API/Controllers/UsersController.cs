// UsersController — handles the logged-in user's own account (read, update, delete)
// All endpoints require a valid JWT token thanks to [Authorize]

using BeerJournal.Model.Entities;
using BeerJournal.Model.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BeerJournal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]   // every endpoint below requires a valid JWT token
public class UsersController : ControllerBase
{
    private readonly UserRepository _userRepository;

    public UsersController(UserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    // GET /api/Users/me — return the logged-in user's info
    // Used by the settings page to show the user's current details
    [HttpGet("me")]
    public IActionResult GetMe()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var user = _userRepository.GetUserById(userId.Value);
        if (user == null) return NotFound();

        return Ok(user);
    }

    // PUT /api/Users/me — update the logged-in user's info (name, email)
    [HttpPut("me")]
    public IActionResult UpdateMe([FromBody] User user)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        // Force the userId from the token — so a user can only update themselves
        user.UserId = userId.Value;

        var result = _userRepository.UpdateUser(user);
        if (!result) return NotFound();

        return Ok(new { message = "User updated" });
    }

    // DELETE /api/Users/me — delete the logged-in user's account
    [HttpDelete("me")]
    public IActionResult DeleteMe()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = _userRepository.DeleteUser(userId.Value);
        if (!result) return NotFound();

        return Ok(new { message = "User deleted" });
    }

    // Helper — reads the userId from the JWT token's claims
    // This tells us who is logged in without needing the client to send it
    private int? GetUserId()
    {
        var claim = User.FindFirst("userId")?.Value;
        if (claim == null) return null;
        return int.Parse(claim);
    }
}