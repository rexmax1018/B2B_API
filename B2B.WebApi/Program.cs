using Autofac;
using Autofac.Extensions.DependencyInjection;
using B2B.Service.Interfaces;
using B2B.WebApi.Modules;
using B2B.WebApi.Extensions;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using NLog;
using NLog.Web;

var logger = LogManager.Setup().LoadConfigurationFromFile("nlog.config").GetCurrentClassLogger();

try
{
    logger.Info("B2B_API 正在啟動。");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
    builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
        containerBuilder.RegisterModule<B2BWebApiModule>());

    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    builder.Services
        .AddB2BOptions(builder.Configuration)
        .AddB2BAuthentication(builder.Configuration)
        .AddB2BRateLimiting(builder.Configuration)
        .AddB2BSwagger();

    var app = builder.Build();

    SecurityConfigurationValidator.Validate(app);
    var entryCredentialValidator = app.Services.GetRequiredService<IEntryCredentialValidator>();

    if (!app.Environment.IsDevelopment() && entryCredentialValidator.IsDevelopmentFixture)
    {
        throw new InvalidOperationException(
            "Entry.ini 仍是公開開發範例，非 Development 環境必須改用專屬的 AES-GCM 密文。");
    }

    app.Lifetime.ApplicationStarted.Register(() =>
        logger.Info(
            "B2B_API 已啟動。環境：{0}；內容根目錄：{1}；URL：{2}",
            app.Environment.EnvironmentName,
            app.Environment.ContentRootPath,
            string.Join(", ", app.Urls)));

    app.Lifetime.ApplicationStopping.Register(() =>
        logger.Info("B2B_API 正在停止。"));

    app.Lifetime.ApplicationStopped.Register(() =>
        logger.Info("B2B_API 已停止。"));

    app.UseB2BForwardedHeaders();

    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
    }

    app.UseB2BSecurityHeaders();
    app.UseB2BExceptionHandling();
    app.UseB2BTransactionLog();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseRateLimiter();

    app.MapHealthChecks("/health/live", new HealthCheckOptions
    {
        Predicate = healthCheck => healthCheck.Tags.Contains("live")
    }).AllowAnonymous();

    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = healthCheck => healthCheck.Tags.Contains("ready")
    }).AllowAnonymous();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    logger.Fatal(ex, "B2B_API 非預期停止。");
    throw;
}
finally
{
    LogManager.Shutdown();
}

/// <summary>
/// ASP.NET Core 應用程式進入點，供整合測試建立主機使用。
/// </summary>
public partial class Program;
