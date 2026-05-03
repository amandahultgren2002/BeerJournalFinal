using BeerJournal.Model.Entities;
using BeerJournal.Model.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BeerJournal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Only logged-in users can use these endpoints
public class UsersController : ControllerBase
{
    private readonly UserRepository _userRepository;

    public UsersController(UserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    // GET api/Users/me
    // Gets the currently logged-in user's information
    [HttpGet("me")]
    public IActionResult GetMe()
    {
        var userId = GetUserId(); // Get userId from JWT token
        if (userId == null) return Unauthorized();

        var user = _userRepository.GetUserById(userId.Value);

        if (user == null) return NotFound();

        return Ok(user);
    }

    // PUT api/Users/me
    // Updates the logged-in user's information (first name, last name, email)
    [HttpPut("me")]
    public IActionResult UpdateMe([FromBody] User user)
    {
        var userId = GetUserId(); // Get userId from token
        if (userId == null) return Unauthorized();

        user.UserId = userId.Value; // Make sure user can only update themselves

        var result = _userRepository.UpdateUser(user);

        if (!result) return NotFound();

        return Ok(new { message = "User updated" });
    }

    // DELETE api/Users/me
    // Deletes the logged-in user's account
    [HttpDelete("me")]
    public IActionResult DeleteMe()
    {
        var userId = GetUserId(); // Get userId from token
        if (userId == null) return Unauthorized();

        var result = _userRepository.DeleteUser(userId.Value);

        if (!result) return NotFound();

        return Ok(new { message = "User deleted" });
    }

    // Helper method
    // Reads the userId from the JWT token (this tells us who is logged in)
    private int? GetUserId()
    {
        var claim = User.FindFirst("userId")?.Value;

        if (claim == null) return null;

        return int.Parse(claim);
    }
}