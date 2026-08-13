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
    public DbSet<UserEntity> Users => Set<UserEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new UserEntityMapping());
    }
}
