using Autofac;
using B2B.Dao.Contexts;
using B2B.Dao.Repositories.Implements;
using B2B.Dao.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace B2B.Dao.Modules;

public sealed class B2BDaoModule : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.Register(context =>
            {
                var configuration = context.Resolve<IConfiguration>();
                var connectionString = configuration["ConnectionStrings:DefaultConnection"];

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required.");
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
