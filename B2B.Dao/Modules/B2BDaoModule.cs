using Autofac;
using B2B.Dao.Contexts;
using B2B.Dao.Repositories.Implements;
using B2B.Dao.Repositories.Interfaces;
using B2B_Conn.Modules;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace B2B.Dao.Modules;

/// <summary>
/// 註冊 Oracle 連線、使用者查詢與健康檢查所需的 Autofac 服務。
/// </summary>
public sealed class B2BDaoModule : Autofac.Module
{
    /// <summary>
    /// 註冊資料庫連線設定、DbContext 與使用者查詢實作。
    /// </summary>
    /// <param name="builder">Autofac 容器建構器。</param>
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterModule<B2BConnModule>();

        builder.Register(context =>
            {
                var configuration = context.Resolve<IConfiguration>();
                var daoOptions = BuildDaoOptions(configuration, context.Resolve<global::B2B_Conn.B2B_Conn>());

                if (string.IsNullOrWhiteSpace(daoOptions.ConnectionString))
                {
                    throw new InvalidOperationException("必須設定 DataAccess:B2BConn 並確認 B2B.Conn 可取得資料庫連線資訊。");
                }

                return daoOptions;
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

    /// <summary>
    /// 建立 DAO 連線設定。
    /// </summary>
    /// <param name="configuration">應用程式設定。</param>
    /// <param name="b2bConn">B2B.Conn 連線資訊提供者。</param>
    /// <returns>DAO 連線設定。</returns>
    private static B2BDaoOptions BuildDaoOptions(
        IConfiguration configuration,
        global::B2B_Conn.B2B_Conn b2bConn)
    {
        var envType = GetRequiredConfiguration(configuration, "DataAccess:B2BConn:EnvType");
        var svrType = GetRequiredConfiguration(configuration, "DataAccess:B2BConn:SvrType");
        var dbType = GetRequiredConfiguration(configuration, "DataAccess:B2BConn:DBType");
        var accType = GetRequiredConfiguration(configuration, "DataAccess:B2BConn:AccType");
        var entityConnection = b2bConn.GetEntityInfo(envType, svrType, dbType, accType);
        var connectionString = BuildOracleConnectionString(entityConnection);

        return new B2BDaoOptions
        {
            ConnectionString = connectionString,
            EnvType = envType,
            SvrType = svrType,
            DBType = dbType,
            AccType = accType
        };
    }

    /// <summary>
    /// 建立 Oracle EF Core 連線字串。
    /// </summary>
    /// <param name="entityConnection">B2B.Conn 回傳的連線資訊。</param>
    /// <returns>Oracle managed provider 連線字串。</returns>
    private static string BuildOracleConnectionString(global::B2B_Conn.Entity_Connection entityConnection)
    {
        if (string.IsNullOrWhiteSpace(entityConnection.DataSource) ||
            string.IsNullOrWhiteSpace(entityConnection.Acc) ||
            string.IsNullOrWhiteSpace(entityConnection.pwd))
        {
            return string.Empty;
        }

        return $"User Id={entityConnection.Acc};Password={entityConnection.pwd};Data Source={entityConnection.DataSource};Pooling=true;Max Pool Size=100";
    }

    /// <summary>
    /// 取得必要設定值。
    /// </summary>
    /// <param name="configuration">應用程式設定。</param>
    /// <param name="key">設定鍵。</param>
    /// <returns>設定值。</returns>
    private static string GetRequiredConfiguration(IConfiguration configuration, string key)
    {
        var value = configuration[key];

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"必須設定 {key}。");
        }

        return value;
    }
}
