using LifeRPG.Api.Data;
using LifeRPG.Api.Models;
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

        // Наши правила пароля для MVP
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Для API не хотим получать HTML-редирект на страницу Login.
// Хотим нормальные HTTP 401 / 403.

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
        {
            user = await userManager.FindByEmailAsync(request.Login);
        }
        else
        {
            user = await userManager.FindByNameAsync(request.Login);
        }

        if (user is null)
        {
            return Results.Unauthorized();
        }

        var result =
            await signInManager.PasswordSignInAsync(
                user,
                request.Password,
                request.RememberMe,
                lockoutOnFailure: true
            );

        if (!result.Succeeded)
        {
            return Results.Unauthorized();
        }

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
        {
            return Results.Unauthorized();
        }

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

app.Run();