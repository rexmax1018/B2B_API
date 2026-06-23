using Autofac;

namespace B2B_Conn.Modules;

/// <summary>
/// 註冊 B2B.Conn 連線字串與金鑰處理所需的 Autofac 服務。
/// </summary>
public sealed class B2BConnModule : Module
{
    private readonly global::B2B_Conn.B2BConnOptions options;
    private readonly TimeProvider timeProvider;

    public B2BConnModule()
        : this(global::B2B_Conn.B2BConnOptions.Default)
    {
    }

    public B2BConnModule(global::B2B_Conn.B2BConnOptions options)
        : this(options, TimeProvider.System)
    {
    }

    internal B2BConnModule(global::B2B_Conn.B2BConnOptions options, TimeProvider timeProvider)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    /// <summary>
    /// 註冊 B2B.Conn public facade 與內部協作服務。
    /// </summary>
    /// <param name="builder">Autofac 容器建構器。</param>
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterInstance(options)
            .AsSelf()
            .SingleInstance();

        builder.RegisterInstance(timeProvider)
            .AsSelf()
            .SingleInstance();

        builder.RegisterType<global::B2B_Conn.ConnectionProfileProvider>()
            .AsSelf()
            .SingleInstance();

        builder.RegisterType<global::B2B_Conn.IniCredentialStore>()
            .AsSelf()
            .SingleInstance();

        builder.RegisterType<global::B2B_Conn.CredentialResolutionService>()
            .AsSelf()
            .InstancePerLifetimeScope();

        builder.RegisterType<global::B2B_Conn.KeySetProvider>()
            .AsSelf()
            .SingleInstance();

        builder.RegisterType<global::B2B_Conn.RsaPrivateKeyDecryptor>()
            .AsSelf()
            .SingleInstance();

        builder.RegisterType<global::B2B_Conn.SymmetricKeyProvider>()
            .AsSelf()
            .InstancePerLifetimeScope();

        builder.RegisterType<global::B2B_Conn.AesStringProtector>()
            .AsSelf()
            .InstancePerLifetimeScope();

        builder.Register(context => new global::B2B_Conn.B2B_Conn(
                context.Resolve<global::B2B_Conn.ConnectionProfileProvider>(),
                context.Resolve<global::B2B_Conn.CredentialResolutionService>()))
            .AsSelf()
            .InstancePerLifetimeScope();
    }
}
