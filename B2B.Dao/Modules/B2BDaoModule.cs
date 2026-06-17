using Autofac;
using B2B.Dao.Contexts;
using B2B.Dao.Repositories.Implements;
using B2B.Dao.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace B2B.Dao.Modules;

/// <summary>
/// 註冊資料存取層所需的 Autofac 服務。
/// </summary>
public sealed class B2BDaoModule : Autofac.Module
{
    /// <summary>
    /// 註冊資料庫連線設定、DbContext 與使用者資料存取實作。
    /// </summary>
    /// <param name="builder">Autofac 容器建構器。</param>
    protected override void Load(ContainerBuilder builder)
    {
        builder.Register(context =>
            {
                var configuration = context.Resolve<IConfiguration>();
                var connectionString = configuration["ConnectionStrings:DefaultConnection"];

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new InvalidOperationException("必須設定 ConnectionStrings:DefaultConnection。");
                }

                return new B2BDaoOptions
                {
                    ConnectionString = connectionString
                };
            })
            .AsSelf()
            .SingleInstance();

        builder.Register(context =>
            {
                var daoOptions = context.Resolve<B2BDaoOptions>();

                return new DbContextOptionsBuilder<B2BDbContext>()
                    .UseOracle(daoOptions.ConnectionString)
                    .Options;
            })
            .As<DbContextOptions<B2BDbContext>>()
            .InstancePerLifetimeScope();

        builder.RegisterType<B2BDbContext>()
            .AsSelf()
            .InstancePerLifetimeScope();

        builder.Register(context =>
            {
                var configuration = context.Resolve<IConfiguration>();
                return bool.TryParse(configuration["DataAccess:UseFakeRepositories"], out var useFakeRepositories)
                    && useFakeRepositories;
            })
            .Keyed<bool>("UseFakeRepositories")
            .SingleInstance();

        builder.RegisterType<InMemoryUserRepository>()
            .Keyed<IUserRepository>("FakeUserRepository")
            .SingleInstance();

        builder.RegisterType<UserRepository>()
            .Keyed<IUserRepository>("UserRepository")
            .InstancePerLifetimeScope();

        builder.Register(context =>
            {
                var useFakeRepositories = context.ResolveKeyed<bool>("UseFakeRepositories");
                return useFakeRepositories
                    ? context.ResolveKeyed<IUserRepository>("FakeUserRepository")
                    : context.ResolveKeyed<IUserRepository>("UserRepository");
            })
            .As<IUserRepository>()
            .InstancePerLifetimeScope();
    }
}
