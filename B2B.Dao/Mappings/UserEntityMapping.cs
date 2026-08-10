using B2B.Dao.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2B.Dao.Mappings;

/// <summary>
/// 設定使用者實體與資料表欄位的對應。
/// </summary>
internal sealed class UserEntityMapping : IEntityTypeConfiguration<UserEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<UserEntity> entity)
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
    }
}
