namespace LifeRPG.Api.Models;

public class Character
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public int Level { get; set; } = 1;
    public int TotalXp { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
