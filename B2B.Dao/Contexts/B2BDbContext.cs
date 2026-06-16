using B2B.Dao.Entities;
using B2B.Dao.Mappings;
using Microsoft.EntityFrameworkCore;

namespace B2B.Dao.Contexts;

public sealed class B2BDbContext(DbContextOptions<B2BDbContext> options) : DbContext(options)
{
    public DbSet<UserEntity> Users => Set<UserEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new UserEntityMapping());
    }
}
