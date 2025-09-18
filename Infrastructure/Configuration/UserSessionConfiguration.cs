using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Configuration
{
    public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
    {
        public void Configure(EntityTypeBuilder<UserSession> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.UserId)
                  .IsRequired();

            builder.Property(e => e.RefreshToken)
                  .HasMaxLength(256)
                  .IsRequired();

            builder.Property(e => e.IpAddress)
                  .HasMaxLength(45); // IPv6 max length

            builder.Property(e => e.UserAgent)
                  .HasMaxLength(512);

            builder.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(e => e.UserId);
            builder.HasIndex(e => e.RefreshToken).IsUnique();
            builder.HasIndex(e => e.ExpiresAt);
        }
    }
}
