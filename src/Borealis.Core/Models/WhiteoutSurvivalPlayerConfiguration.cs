using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Borealis.Core.Models;

public class WhiteoutSurvivalPlayerConfiguration : IEntityTypeConfiguration<WhiteoutSurvivalPlayer> {
    public void Configure(EntityTypeBuilder<WhiteoutSurvivalPlayer> builder) {
        builder.HasKey(b => b.Id);

        builder.HasAlternateKey(b => b.ExternalId);
        builder.HasIndex(b => b.ExternalId).IsUnique();

        builder.Property(b => b.CreatedAt).IsRequired();

        builder.Property(b => b.UpdatedAt).IsRequired();

        builder.Property(b => b.ExternalId).IsRequired();

        builder.Property(b => b.Name).IsRequired().HasMaxLength(128);

        builder.Property(b => b.State).IsRequired();

        builder.Property(b => b.FurnaceLevel).IsRequired();

        builder.Property(b => b.IsInAlliance).IsRequired();

        builder.OwnsMany(b => b.PreviousNames, navigationBuilder => navigationBuilder.ToJson());

        builder.Property(b => b.Notes);

        builder.Ignore(b => b.FurnaceLevelString);
    }
}
