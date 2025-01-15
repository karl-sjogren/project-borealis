using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Borealis.Core.Models;

public class UserConfiguration : IEntityTypeConfiguration<User> {
    public void Configure(EntityTypeBuilder<User> builder) {
        builder.HasKey(b => b.Id);

        builder.ToTable(options => options.IsTemporal());

        builder.Property(b => b.CreatedAt).IsRequired();

        builder.Property(b => b.UpdatedAt).IsRequired();

        builder.Property(b => b.Name).IsRequired().HasMaxLength(128);

        builder.Property(b => b.ExternalId).IsRequired().HasMaxLength(128);

        builder.Property(b => b.IsApproved).IsRequired();

        builder.Property(b => b.IsLockedOut).IsRequired();

        builder.Property(b => b.IsAdmin).IsRequired();
    }
}
