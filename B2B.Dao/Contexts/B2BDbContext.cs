using B2B.Dao.Entities;
using Microsoft.EntityFrameworkCore;

namespace B2B.Dao.Contexts;

public sealed class B2BDbContext(DbContextOptions<B2BDbContext> options) : DbContext(options)
{
    public DbSet<UserEntity> Users => Set<UserEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.ToTable("B2B_USER");
            entity.HasKey(x => x.UserId);

            entity.Property(x => x.UserId).HasColumnName("USER_ID").ValueGeneratedOnAdd();
            entity.Property(x => x.Account).HasColumnName("ACCOUNT").HasMaxLength(100).IsRequired();
            entity.Property(x => x.DisplayName).HasColumnName("DISPLAY_NAME").HasMaxLength(200).IsRequired();
            entity.Property(x => x.PasswordHash).HasColumnName("PASSWORD_HASH").HasMaxLength(500).IsRequired();
            entity.Property(x => x.IsActive).HasColumnName("IS_ACTIVE").HasConversion<int>().IsRequired();
            entity.Property(x => x.CreatedAt).HasColumnName("CREATED_AT").IsRequired();
            entity.Property(x => x.UpdatedAt).HasColumnName("UPDATED_AT");

            entity.HasIndex(x => x.Account).HasDatabaseName("UX_B2B_USER_ACCOUNT").IsUnique();
        });
    }
}
