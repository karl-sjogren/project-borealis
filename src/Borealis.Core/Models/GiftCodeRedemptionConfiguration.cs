using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Borealis.Core.Models;

public class GiftCodeRedemptionConfiguration : IEntityTypeConfiguration<GiftCodeRedemption> {
    public void Configure(EntityTypeBuilder<GiftCodeRedemption> builder) {
        builder.HasKey(b => b.Id);

        builder.ToTable(options => options.IsTemporal());

        builder.Property(b => b.RedeemedAt).IsRequired();

        builder.HasOne(b => b.Player).WithMany().IsRequired().HasForeignKey(b => b.PlayerId);

        builder.HasOne(b => b.GiftCode).WithMany(x => x.Redemptions).IsRequired().HasForeignKey(b => b.GiftCodeId);
    }
}
