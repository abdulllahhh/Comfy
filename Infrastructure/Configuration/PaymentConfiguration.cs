using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Model.Entities;
using Microsoft.EntityFrameworkCore.SqlServer;


namespace Infrastructure.Configuration
{
    public class PaymentConfiguration: IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Amount).HasColumnType("decimal(10,2)");

            builder.HasOne(p => p.User)
                   .WithMany(u => u.Payments)
                   .HasForeignKey(p => p.UserId);
            
            builder.Property(e => e.Amount)
                  .HasColumnType("decimal(18,2)")
                  .IsRequired();

            builder.Property(e => e.Currency)
                  .HasMaxLength(3)
                  .HasDefaultValue("USD");

            builder.Property(e => e.Status)
                  .HasMaxLength(50)
                  .IsRequired();

            builder.Property(e => e.StripeSessionId)
                  .HasMaxLength(255);


            builder.Property(e => e.StripePaymentIntentId)
                  .HasMaxLength(255);

            builder.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(e => e.StripeSessionId).IsUnique();
            builder.HasIndex(e => e.UserId);
        }
    }
}
