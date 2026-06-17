using Autofac;
using B2B.Dao.Modules;
using B2B.Service.Impl.Modules;

namespace B2B.WebApi.Modules;

/// <summary>
/// 註冊 Web API 所需的 Autofac 模組。
/// </summary>
public sealed class B2BWebApiModule : Autofac.Module
{
    /// <summary>
    /// 註冊 Web API 依賴的資料存取層與服務層模組。
    /// </summary>
    /// <param name="builder">Autofac 容器建構器。</param>
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterModule<B2BDaoModule>();
        builder.RegisterModule<B2BServiceModule>();
    }
}
