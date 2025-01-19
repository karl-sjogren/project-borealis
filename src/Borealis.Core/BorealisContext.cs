using Borealis.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Borealis.Core;

public class BorealisContext : DbContext {
    public BorealisContext(DbContextOptions<BorealisContext> options)
        : base(options) {
    }

    public DbSet<Player> Players { get; set; }
    public DbSet<GiftCode> GiftCodes { get; set; }
    public DbSet<GiftCodeRedemption> GiftCodeRedemptions { get; set; }
    public DbSet<MessageTemplate> MessageTemplates { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder
            .Properties<DateTimeOffset>()
            .HaveConversion<DateTimeOffsetToBinaryConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BorealisContext).Assembly);
    }
}
