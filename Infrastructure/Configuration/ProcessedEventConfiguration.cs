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
    public class ProcessedEventConfiguration : IEntityTypeConfiguration<ProcessedEvent>
    {
        public void Configure(EntityTypeBuilder<ProcessedEvent> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.StripeEventId)
                  .HasMaxLength(255)
                  .IsRequired();

            builder.Property(e => e.EventType)
                  .HasMaxLength(100)
                  .IsRequired();

            builder.Property(e => e.ProcessedAt)
                  .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(e => e.UserId)
                  .HasMaxLength(450); // Same as Idbuilder UserId

            builder.HasIndex(e => e.StripeEventId).IsUnique();
            builder.HasIndex(e => e.ProcessedAt);
        }
    }
}
