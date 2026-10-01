namespace LifeRPG.Api.Contracts.Auth;

public record LoginRequest(
    string Login,
    string Password,
    bool RememberMe = false
);