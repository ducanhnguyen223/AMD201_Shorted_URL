using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.AllowAnyOrigin()
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

// 1. Add Ocelot configuration
var ocelotConfigFile = builder.Environment.EnvironmentName == "Production"
    ? "ocelot.Production.json"
    : (builder.Environment.EnvironmentName == "Development" && Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true"
        ? "ocelot.Docker.json"
        : "ocelot.json");

builder.Configuration.AddJsonFile(ocelotConfigFile, optional: false, reloadOnChange: true);

// 2. Add JWT Authentication services
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"];
var issuer = jwtSettings["Issuer"];
var audience = jwtSettings["Audience"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

app.UseCors(MyAllowSpecificOrigins);

// Temporary endpoint to generate a test token
app.MapGet("/get-token", () => {
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    var secretKey = jwtSettings["Secret"];
    var issuer = jwtSettings["Issuer"];
    var audience = jwtSettings["Audience"];

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
        issuer: issuer,
        audience: audience,
        expires: DateTime.Now.AddHours(1),
        signingCredentials: creds
    );

    return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
});

// Health endpoint for API Gateway
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "ApiGateway", timestamp = DateTime.UtcNow }));

// Configure forwarded headers for downstream services
app.Use(async (context, next) =>
{
    context.Request.Headers.Append("X-Forwarded-Host", context.Request.Host.Value);
    context.Request.Headers.Append("X-Forwarded-Proto", context.Request.Scheme);
    await next();
});

// Use Authentication middleware - IMPORTANT: Must be before UseOcelot()
app.UseAuthentication();
app.UseAuthorization();

app.UseOcelot().Wait();

app.Run();

