using Autofac;
using B2B.Dao.Modules;
using B2B.Service.Impl.Modules;

namespace B2B.WebApi.Modules;

public sealed class B2BWebApiModule : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterModule<B2BDaoModule>();
        builder.RegisterModule<B2BServiceModule>();
    }
}
