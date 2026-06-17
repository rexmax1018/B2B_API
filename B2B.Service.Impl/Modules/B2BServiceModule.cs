using Autofac;
using B2B.Service.Impl.Services;
using B2B.Service.Impl.Stores;
using B2B.Service.Interfaces;

namespace B2B.Service.Impl.Modules;

/// <summary>
/// 註冊服務層實作的 Autofac 服務。
/// </summary>
public sealed class B2BServiceModule : Autofac.Module
{
    /// <summary>
    /// 註冊驗證、使用者、權杖與 Refresh Token 儲存服務。
    /// </summary>
    /// <param name="builder">Autofac 容器建構器。</param>
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<AuthService>()
            .As<IAuthService>()
            .InstancePerLifetimeScope();

        builder.RegisterType<UserService>()
            .As<IUserService>()
            .InstancePerLifetimeScope();

        builder.RegisterType<TokenService>()
            .As<ITokenService>()
            .InstancePerLifetimeScope();

        builder.RegisterType<MemoryRefreshTokenStore>()
            .As<IRefreshTokenStore>()
            .InstancePerLifetimeScope();
    }
}
