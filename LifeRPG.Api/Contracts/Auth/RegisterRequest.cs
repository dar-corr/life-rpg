namespace LifeRPG.Api.Contracts.Auth;

public record RegisterRequest(
    string Username,
    string Email,
    string Password
);