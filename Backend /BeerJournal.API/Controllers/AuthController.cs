using BeerJournal.Model.Entities;
using BeerJournal.Model.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BeerJournal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserRepository _userRepository;
    private readonly IConfiguration _config;

    public AuthController(UserRepository userRepository, IConfiguration config)
    {
        _userRepository = userRepository;
        _config = config;
    }

    // POST api/Auth/register
    // Creates a new user with a hashed password
    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest req)
    {
        // Validate all fields are filled
        if (string.IsNullOrWhiteSpace(req.FirstName) ||
            string.IsNullOrWhiteSpace(req.LastName)  ||
            string.IsNullOrWhiteSpace(req.Email)     ||
            string.IsNullOrWhiteSpace(req.Password))
            return BadRequest("All fields are required");

        // Check email is not already taken
        var existing = _userRepository.GetUserByEmail(req.Email);
        if (existing != null)
            return Conflict("An account with this email already exists");

        // Build the user object and register
        var user = new User
        {
            FirstName = req.FirstName,
            LastName  = req.LastName,
            Email     = req.Email
        };

        var result = _userRepository.RegisterUser(user, req.Password);

        if (!result)
            return BadRequest("Could not create account");

        return Ok(new { message = "Account created successfully" });
    }

    // POST api/Auth/login
    // Validates credentials and returns a JWT token
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest("Email and password are required");

        // Validate email + password against the database
        var user = _userRepository.ValidateLogin(req.Email, req.Password);

        if (user == null)
            return Unauthorized("Invalid email or password");

        // Generate JWT token
        var token = GenerateToken(user);

        // Return token + first name so the frontend can greet the user
        return Ok(new
        {
            token,
            firstName = user.FirstName,
            userId    = user.UserId,
            email     = user.Email
        });
    }

    // Generates a JWT token containing user info as claims
    private string GenerateToken(User user)
    {
        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Claims are pieces of info stored inside the token
        // The frontend and backend can read these without hitting the database
        var claims = new[]
        {
            new Claim("userId",    user.UserId.ToString()),
            new Claim("email",     user.Email),
            new Claim("firstName", user.FirstName)
        };

        var token = new JwtSecurityToken(
            claims:             claims,
            expires:            DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

// Request body for register
public class RegisterRequest
{
    public string FirstName { get; set; } = null!;
    public string LastName  { get; set; } = null!;
    public string Email     { get; set; } = null!;
    public string Password  { get; set; } = null!;
}

// Request body for login
public class LoginRequest
{
    public string Email    { get; set; } = null!;
    public string Password { get; set; } = null!;
}