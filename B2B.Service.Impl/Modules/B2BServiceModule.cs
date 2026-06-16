using Autofac;
using B2B.Service.Impl.Services;
using B2B.Service.Interfaces;

namespace B2B.Service.Impl.Modules;

public sealed class B2BServiceModule : Autofac.Module
{
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
    }
}
