using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Borealis.Core.Models;

public class PlayerConfiguration : IEntityTypeConfiguration<Player> {
    public void Configure(EntityTypeBuilder<Player> builder) {
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
        builder.Property(b => b.ForceRedeemGiftCodes).IsRequired();

        builder.OwnsMany(b => b.PreviousNames, navigationBuilder => navigationBuilder.ToJson());

        builder.Property(b => b.Notes);

        builder.Property(b => b.AwayUntil);

        builder.Ignore(b => b.FurnaceLevelString);

        builder.Ignore(b => b.HasNotes);
    }
}
