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
                  .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                  .IsRequired()
                  ;
            builder.Property(u => u.ExpiresAt).HasColumnType("datetime(6)").IsRequired();

            builder.Property(u => u.IsRevoked)
                   .HasColumnType("tinyint(1)")
                   .HasDefaultValue(false)
                   .IsRequired();

            builder.HasIndex(e => e.UserId);
            builder.HasIndex(e => e.RefreshToken).IsUnique();
            builder.HasIndex(e => e.ExpiresAt);
        }
    }
}
