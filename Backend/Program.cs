using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using UserStore.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("Users") ?? "Data Source=Users.db";

// Add services to the container.
builder.Services.AddSqlite<UserDbContext>(connectionString);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "UserStore API", Version = "v1" });
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var secretKey = "Your_Super_Secret_Key_That_Is_At_Least_32_Characters_Long";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "UserStore API V1");
});

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "Hello World!");

app.MapPost("/signup", async (UserDbContext db, User user) =>
{
    if (await db.Users.AnyAsync(u => u.Email == user.Email))
    {
        return Results.BadRequest("User already exists.");
    }

    user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
    db.Users.Add(user);
    await db.SaveChangesAsync();

    return Results.Created($"/users/{user.Id}", user);
});

app.MapPost("/login", async (UserDbContext db, User loginRequest) =>
{
    var user = await db.Users.SingleOrDefaultAsync(u => u.Email == loginRequest.Email);

    if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.Password))
    {
        return Results.Unauthorized();
    }

    var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
    var key = Encoding.UTF8.GetBytes(secretKey);
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Expires = DateTime.UtcNow.AddHours(1),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };

    var token = tokenHandler.CreateToken(tokenDescriptor);
    var tokenString = tokenHandler.WriteToken(token);

    return Results.Ok(new { Token = tokenString });
});

app.MapGet("/users", async (UserDbContext db) => await db.Users.ToListAsync());
app.MapGet("/users/{id}", async (UserDbContext db, int id) =>
{
    var user = await db.Users.FindAsync(id);
    return user is not null ? Results.Ok(user) : Results.NotFound();
});

app.MapPut("/users/{id}", async (UserDbContext db, int id, User updatedUser) =>
{
    var user = await db.Users.FindAsync(id);

    if (user is null)
    {
        return Results.NotFound();
    }

    user.Email = updatedUser.Email;
    user.Password = BCrypt.Net.BCrypt.HashPassword(updatedUser.Password);

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/users/{id}", async (UserDbContext db, int id) =>
{
    var user = await db.Users.FindAsync(id);

    if (user is null)
    {
        return Results.NotFound();
    }

    db.Users.Remove(user);
    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.Run();
