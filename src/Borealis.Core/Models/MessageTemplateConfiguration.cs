using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Borealis.Core.Models;

public class MessageTemplateConfiguration : IEntityTypeConfiguration<MessageTemplate> {
    public void Configure(EntityTypeBuilder<MessageTemplate> builder) {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.CreatedAt).IsRequired();

        builder.Property(b => b.UpdatedAt).IsRequired();

        builder.Property(b => b.Name).IsRequired().HasMaxLength(128);

        builder.Property(b => b.Message).IsRequired();

        builder.OwnsMany(b => b.HistorialMessages, navigationBuilder => navigationBuilder.ToJson());
    }
}
