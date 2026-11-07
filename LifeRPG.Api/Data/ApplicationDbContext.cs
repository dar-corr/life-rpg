using LifeRPG.Api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LifeRPG.Api.Data;

public class ApplicationDbContext
    : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Character> Characters => Set<Character>();
    public DbSet<Quest> Quests => Set<Quest>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Character>(entity =>
        {
            entity.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasIndex(c => c.UserId)
                .IsUnique();

            entity.HasOne(c => c.User)
                .WithOne()
                .HasForeignKey<Character>(c => c.UserId);
        });

        builder.Entity<Quest>(entity =>
        {
            entity.Property(q => q.Title)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(q => q.Description)
                .HasMaxLength(4000);

            entity.Property(q => q.Status)
                .IsRequired()
                .HasMaxLength(30);

            entity.HasOne(q => q.Character)
                .WithMany()
                .HasForeignKey(q => q.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(q => q.CharacterId);
        });
    }
}
