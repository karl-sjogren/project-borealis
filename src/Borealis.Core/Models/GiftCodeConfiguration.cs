using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Borealis.Core.Models;

public class GiftCodeConfiguration : IEntityTypeConfiguration<GiftCode> {
    public void Configure(EntityTypeBuilder<GiftCode> builder) {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.CreatedAt).IsRequired();

        builder.Property(b => b.UpdatedAt).IsRequired();

        builder.Property(b => b.Code).IsRequired().HasMaxLength(128);

        builder.Property(b => b.IsExpired).IsRequired();

        builder.HasMany(b => b.Redemptions).WithOne();
    }
}
