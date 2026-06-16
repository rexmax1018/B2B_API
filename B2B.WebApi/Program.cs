using Autofac;
using Autofac.Extensions.DependencyInjection;
using B2B.WebApi.Modules;
using B2B.WebApi.Extensions;
using NLog;
using NLog.Web;

var logger = LogManager.Setup().LoadConfigurationFromFile("nlog.config").GetCurrentClassLogger();

try
{
    logger.Info("B2B_API is starting.");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
    builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
        containerBuilder.RegisterModule<B2BWebApiModule>());

    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    builder.Services
        .AddB2BOptions(builder.Configuration)
        .AddB2BAuthentication(builder.Configuration)
        .AddB2BRateLimiting()
        .AddB2BSwagger();

    var app = builder.Build();

    SecurityConfigurationValidator.Validate(app);

    app.Lifetime.ApplicationStarted.Register(() =>
        logger.Info(
            "B2B_API started. Environment: {0}; ContentRoot: {1}; URLs: {2}",
            app.Environment.EnvironmentName,
            app.Environment.ContentRootPath,
            string.Join(", ", app.Urls)));

    app.Lifetime.ApplicationStopping.Register(() =>
        logger.Info("B2B_API is stopping."));

    app.Lifetime.ApplicationStopped.Register(() =>
        logger.Info("B2B_API stopped."));

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

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    logger.Fatal(ex, "B2B_API stopped unexpectedly.");
    throw;
}
finally
{
    LogManager.Shutdown();
}
