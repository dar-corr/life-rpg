using LifeRPG.Api.Data;
using LifeRPG.Api.Models;
using LifeRPG.Api.Contracts.Characters;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using LifeRPG.Api.Contracts.Auth;

var builder = WebApplication.CreateBuilder(args);

// --------------------
// DATABASE
// --------------------

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// --------------------
// IDENTITY
// --------------------

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;

        // правила пароля для MVP
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();


builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;

    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode =
            StatusCodes.Status401Unauthorized;

        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode =
            StatusCodes.Status403Forbidden;

        return Task.CompletedTask;
    };
});

// --------------------
// CORS
// --------------------

builder.Services.AddCors(options =>
{
    options.AddPolicy("React", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseCors("React");

app.UseAuthentication();
app.UseAuthorization();

// endpoints добавим ниже

app.MapPost(
    "/api/auth/register",
    async (
        RegisterRequest request,
        UserManager<ApplicationUser> userManager
    ) =>
    {
        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return Results.BadRequest(
                new { message = "Все поля обязательны." }
            );
        }

        var user = new ApplicationUser
        {
            UserName = request.Username.Trim(),
            Email = request.Email.Trim()
        };

        var result = await userManager.CreateAsync(
            user,
            request.Password
        );

        if (!result.Succeeded)
        {
            return Results.BadRequest(new
            {
                errors = result.Errors.Select(e => e.Description)
            });
        }

        return Results.Ok(new
        {
            message = "Пользователь зарегистрирован.",
            user.Id,
            user.UserName,
            user.Email
        });
    }
);

app.MapPost(
    "/api/auth/login",
    async (
        LoginRequest request,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager
    ) =>
    {
        ApplicationUser? user;

        if (request.Login.Contains('@'))
            user = await userManager.FindByEmailAsync(request.Login);
        else
            user = await userManager.FindByNameAsync(request.Login);

        if (user is null)
            return Results.Unauthorized();

        var result =
            await signInManager.PasswordSignInAsync(
                user,
                request.Password,
                request.RememberMe,
                lockoutOnFailure: true
            );

        if (!result.Succeeded)
            return Results.Unauthorized();

        return Results.Ok(new
        {
            user.Id,
            user.UserName,
            user.Email
        });
    }
);

app.MapGet(
    "/api/auth/me",
    async (
        HttpContext context,
        UserManager<ApplicationUser> userManager
    ) =>
    {
        var user =
            await userManager.GetUserAsync(context.User);

        if (user is null)
            return Results.Unauthorized();

        return Results.Ok(new
        {
            user.Id,
            user.UserName,
            user.Email,
            user.CreatedAt
        });
    }
)
.RequireAuthorization();

app.MapPost(
    "/api/auth/logout",
    async (
        SignInManager<ApplicationUser> signInManager
    ) =>
    {
        await signInManager.SignOutAsync();

        return Results.NoContent();
    }
)
.RequireAuthorization();


app.MapPost("/api/characters", async (
    CreateCharacterRequest request,
    HttpContext context,
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext db) =>
{
    var user = await userManager.GetUserAsync(context.User);

    if (user is null)
        return Results.Unauthorized();

    var name = request.Name?.Trim();

    if (string.IsNullOrWhiteSpace(name) || name.Length > 100)
        return Results.BadRequest(new
        {
            message = "Имя должно содержать от 1 до 100 символов."
        });

    var exists = await db.Characters
        .AnyAsync(c => c.UserId == user.Id);

    if (exists)
        return Results.Conflict(new
        {
            message = "Персонаж уже существует."
        });

    var character = new Character
    {
        UserId = user.Id,
        Name = name
    };

    db.Characters.Add(character);
    await db.SaveChangesAsync();

    return Results.Created("/api/characters/me", new
    {
        character.Id,
        character.Name,
        character.Level,
        character.TotalXp,
        character.CreatedAt
    });
})
.RequireAuthorization();


app.MapGet("/api/characters/me", async (
    HttpContext context,
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext db) =>
{
    var user = await userManager.GetUserAsync(context.User);

    if (user is null)
        return Results.Unauthorized();

    var character = await db.Characters
        .AsNoTracking()
        .Where(c => c.UserId == user.Id)
        .Select(c => new
        {
            c.Id,
            c.Name,
            c.Level,
            c.TotalXp,
            c.CreatedAt
        })
        .FirstOrDefaultAsync();

    if (character is null)
        return Results.NotFound(new
        {
            message = "Персонаж не найден."
        });

    return Results.Ok(character);
})
.RequireAuthorization();

app.Run();