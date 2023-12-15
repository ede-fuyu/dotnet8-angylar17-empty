using Dotnet8App.EFCore.EntityTables;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Dotnet8App.EFCore;

public partial class ApplicationDbContext(DbContextOptions options) : IdentityDbContext(options)
{
    public virtual DbSet<AppLogs> AppLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppLogs>(entity =>
        {
            entity.Property(e => e.Level).HasMaxLength(128);
            entity.Property(e => e.TimeStamp).HasColumnType("datetime");
            entity.Property(e => e.UserName).HasMaxLength(256);
        });

        base.OnModelCreating(modelBuilder);
    }
}
