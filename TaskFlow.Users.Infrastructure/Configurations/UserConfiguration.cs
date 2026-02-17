using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Users.Domain.Entities;

namespace TaskFlow.Users.Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        // Email - Value Object
        builder.OwnsOne(u => u.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("Email")
                .HasMaxLength(256)
                .IsRequired();

            email.HasIndex(e => e.Value).IsUnique();
        });

        // Password - Value Object
        builder.OwnsOne(u => u.Password, password =>
        {
            password.Property(p => p.HashedValue)
                .HasColumnName("PasswordHash")
                .HasMaxLength(512)
                .IsRequired();
        });

        // FullName - Value Object
        builder.OwnsOne(u => u.FullName, fullName =>
        {
            fullName.Property(f => f.FirstName)
                .HasColumnName("FirstName")
                .HasMaxLength(100)
                .IsRequired();

            fullName.Property(f => f.LastName)
                .HasColumnName("LastName")
                .HasMaxLength(100)
                .IsRequired();
        });

        builder.Property(u => u.IsActive).IsRequired();
        builder.Property(u => u.CreatedAt).IsRequired();

        builder.Ignore(u => u.DomainEvents);
    }
}