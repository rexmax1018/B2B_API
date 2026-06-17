using B2B.Dao.Entities;
using B2B.Dao.Mappings;
using Microsoft.EntityFrameworkCore;

namespace B2B.Dao.Contexts;

/// <summary>
/// B2B 系統資料庫內容。
/// </summary>
/// <param name="options">資料庫內容選項。</param>
public sealed class B2BDbContext(DbContextOptions<B2BDbContext> options) : DbContext(options)
{
    /// <summary>
    /// 取得使用者資料集。
    /// </summary>
    public DbSet<UserEntity> Users => Set<UserEntity>();

    /// <summary>
    /// 設定 B2B 資料模型與資料表對應。
    /// </summary>
    /// <param name="modelBuilder">資料模型建構器。</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new UserEntityMapping());
    }
}
