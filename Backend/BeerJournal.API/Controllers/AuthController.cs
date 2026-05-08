// AuthController — handles register and login HTTP requests

using BeerJournal.Model.Entities;
using BeerJournal.Model.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BeerJournal.API.Controllers;

[ApiController]
[Route("api/[controller]")]   // URL becomes /api/auth
public class AuthController : ControllerBase
{
    // Dependencies — ASP.NET passes these in automatically
    private readonly UserRepository _userRepository;
    private readonly IConfiguration _config;

    public AuthController(UserRepository userRepository, IConfiguration config)
    {
        _userRepository = userRepository;
        _config = config;
    }

    // POST /api/auth/register — create a new user
    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest req)
    {
        // Make sure all fields were sent
        if (string.IsNullOrWhiteSpace(req.FirstName) ||
            string.IsNullOrWhiteSpace(req.LastName)  ||
            string.IsNullOrWhiteSpace(req.Email)     ||
            string.IsNullOrWhiteSpace(req.Password)  ||
            req.ZipCode == null)
            return BadRequest("All fields are required");

        // Check if email is already in use
        var existing = _userRepository.GetUserByEmail(req.Email);
        if (existing != null)
            return Conflict("An account with this email already exists");

        // Build the user object
        var user = new User
        {
            FirstName = req.FirstName,
            LastName  = req.LastName,
            Email     = req.Email,
            ZipCode   = req.ZipCode
        };

        // Save in database (password gets hashed inside the repository)
        var result = _userRepository.RegisterUser(user, req.Password);

        if (!result)
            return BadRequest("Could not create account");

        return Ok(new { message = "Account created successfully" });
    }

    // POST /api/auth/login — check credentials and return a JWT token
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest("Email and password are required");

        // Check email + password against the database
        var user = _userRepository.ValidateLogin(req.Email, req.Password);

        if (user == null)
            return Unauthorized("Invalid email or password");

        // Build a JWT token for the user
        var token = GenerateToken(user);

        // Return the token plus some user info for the frontend
        return Ok(new
        {
            token,
            firstName = user.FirstName,
            userId    = user.UserId,
            email     = user.Email
        });
    }

    // Helper — creates a signed JWT token containing user info
    private string GenerateToken(User user)
    {
        // Secret key from appsettings.json, used to sign the token
        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Claims = info stored inside the token
        var claims = new[]
        {
            new Claim("userId",    user.UserId.ToString()),
            new Claim("email",     user.Email),
            new Claim("firstName", user.FirstName)
        };

        // Token valid for 8 hours
        var token = new JwtSecurityToken(
            claims:             claims,
            expires:            DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        // Convert to the string format the frontend will store
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

// Shape of the JSON body for register
public class RegisterRequest
{
    public string FirstName { get; set; } = null!;
    public string LastName  { get; set; } = null!;
    public string Email     { get; set; } = null!;
    public int?   ZipCode   { get; set; }
    public string Password  { get; set; } = null!;
}

// Shape of the JSON body for login
public class LoginRequest
{
    public string Email    { get; set; } = null!;
    public string Password { get; set; } = null!;
}