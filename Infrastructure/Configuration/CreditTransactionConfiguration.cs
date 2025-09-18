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
    public class CreditTransactionConfiguration : IEntityTypeConfiguration<CreditTransaction>
    {
        public void Configure(EntityTypeBuilder<CreditTransaction> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.UserId)
                  .IsRequired();

            builder.Property(e => e.Amount)
                  .IsRequired();

            builder.Property(e => e.TransactionType)
                  .HasMaxLength(50)
                  .IsRequired();

            builder.Property(e => e.Description)
                  .HasMaxLength(255);

            builder.Property(e => e.ReferenceId)
                  .HasMaxLength(100);

            builder.Property(e => e.Timestamp)
                  .HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(e => e.UserId);
            builder.HasIndex(e => e.TransactionType);
            builder.HasIndex(e => e.Timestamp);
        }
    }
}
