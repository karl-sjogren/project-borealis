using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Borealis.Core.Models;

public class DiscordNotificationSettingsConfiguration : IEntityTypeConfiguration<DiscordNotificationSettings> {
    public void Configure(EntityTypeBuilder<DiscordNotificationSettings> builder) {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.CreatedAt).IsRequired();

        builder.Property(b => b.UpdatedAt).IsRequired();

        builder.Property(b => b.GuildId).HasMaxLength(20);

        builder.Property(b => b.GiftCodeChannelId).HasMaxLength(20);

        builder.Property(b => b.PlayerRenameChannelId).HasMaxLength(20);

        builder.Property(b => b.PlayerFurnaceLevelChannelId).HasMaxLength(20);

        builder.Property(b => b.PlayerMovedStateChannelId).HasMaxLength(20);
    }
}
