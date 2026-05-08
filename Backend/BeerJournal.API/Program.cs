// Program.cs — entry point of the backend
// This is where we configure and start the whole ASP.NET application

using BeerJournal.Model.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

// Create the app builder — used to configure services before the app starts
var builder = WebApplication.CreateBuilder(args);

// ── Basic services ──────────────────────────────────────────
// Tells ASP.NET to look for [ApiController] classes (our controllers)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ── Swagger setup ───────────────────────────────────────────
// Swagger gives us the auto-generated API testing UI at /swagger
builder.Services.AddSwaggerGen(options =>
{
    // Tells Swagger that our API uses JWT Bearer tokens
    // This adds the "Authorize" button at the top of the Swagger UI
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Paste your JWT token here (no 'Bearer' prefix needed)"
    });

    // Apply the JWT requirement to all endpoints marked with [Authorize]
    options.AddSecurityRequirement(document =>
    {
        return new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecuritySchemeReference("Bearer", document),
                new List<string>()
            }
        };
    });
});

// ── Dependency injection — register our repositories ────────
// AddScoped = a new instance is created for each HTTP request
// ASP.NET will automatically pass these into controllers that need them
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<TastingEntryRepository>();
builder.Services.AddScoped<BeerRepository>();

// ── JWT Authentication ──────────────────────────────────────
// Read the secret signing key from appsettings.json
var key = builder.Configuration["Jwt:Key"]
    ?? throw new Exception("Jwt:Key is missing from appsettings.json!");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Use the older token handler — the new one in .NET 10 has stricter
        // parsing that can cause IDX14102 errors with our tokens
        options.UseSecurityTokenValidators = true;

        // Tells ASP.NET how to check that an incoming JWT is valid
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = false,   // we don't care who issued it
            ValidateAudience         = false,   // we don't care about audience
            ValidateLifetime         = true,    // reject expired tokens
            ValidateIssuerSigningKey = true,    // check the signature
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
    });

builder.Services.AddAuthorization();

// ── CORS ────────────────────────────────────────────────────
// CORS = Cross-Origin Resource Sharing
// Browser blocks requests between different origins by default
// Our frontend runs on :4200, our backend on :5296 — different origins,
// so we have to explicitly allow the frontend to call us
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Build the app — services are now locked in
var app = builder.Build();

// ── Middleware pipeline (order matters!) ────────────────────
// Each request passes through these in order from top to bottom
app.UseSwagger();          // serves the /swagger JSON
app.UseSwaggerUI();        // serves the interactive Swagger page

app.UseCors("AllowAngular");   // must come before authentication

app.UseAuthentication();   // checks the JWT token
app.UseAuthorization();    // enforces [Authorize] attributes

app.MapControllers();      // routes URLs to our controller methods

// Start listening for HTTP requests
app.Run();