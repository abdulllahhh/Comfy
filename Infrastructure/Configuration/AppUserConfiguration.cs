using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Model.Entities;

namespace infrastructure.Configuration;

public class AppUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(255);



        builder.HasMany(u => u.Payments)
               .WithOne(p => p.User)
               .HasForeignKey(p => p.UserId);

        builder.Property(e => e.RefreshToken)
                  .HasMaxLength(256);

        builder.Property(e => e.LastLoginDate)
                  .HasColumnType("datetime2");
    }
}
