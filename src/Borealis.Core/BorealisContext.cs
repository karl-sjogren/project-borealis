using Borealis.Core.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Borealis.Core;

public class BorealisContext : IdentityDbContext {
    public BorealisContext(DbContextOptions<BorealisContext> options)
        : base(options) {
    }

    public DbSet<WhiteoutSurvivalPlayer> Players { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BorealisContext).Assembly);
    }
}
