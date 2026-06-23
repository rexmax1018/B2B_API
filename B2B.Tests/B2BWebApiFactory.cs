using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace B2B.Tests;

/// <summary>
/// 建立整合測試用的 Web API 主機。
/// </summary>
public sealed class B2BWebApiFactory : WebApplicationFactory<Program>
{
    private readonly Dictionary<string, string?> previousEnvironmentValues = [];

    /// <summary>
    /// 初始化整合測試主機設定。
    /// </summary>
    public B2BWebApiFactory()
    {
        SetEnvironment("Jwt__Issuer", "B2B_API_TEST");
        SetEnvironment("Jwt__Audience", "B2B_API_TEST_CLIENT");
        SetEnvironment("Jwt__SecretKey", "integration-test-secret-key-with-at-least-32-characters");
        SetEnvironment("Jwt__AccessTokenMinutes", "60");
        SetEnvironment("Jwt__RefreshTokenDays", "7");
        SetEnvironment("DataAccess__UseFakeRepositories", "true");
        SetEnvironment("DataAccess__B2BConn__EnvType", "TEST");
        SetEnvironment("DataAccess__B2BConn__SvrType", "DEV");
        SetEnvironment("DataAccess__B2BConn__DBType", "INET");
        SetEnvironment("DataAccess__B2BConn__AccType", "ASI4");
        SetEnvironment("TransactionLog__Enabled", "false");
        SetEnvironment("TransactionLog__IncludeRequestBody", "false");
        SetEnvironment("TransactionLog__IncludeResponseBody", "false");
        SetEnvironment("TransactionLog__TrustForwardedHeaders", "false");
        SetEnvironment("TransactionLog__MaxBodyLogLength", "10000");
        SetEnvironment("AllowedHosts", "localhost");
    }

    /// <summary>
    /// 設定整合測試主機的執行環境與記憶體設定來源。
    /// </summary>
    /// <param name="builder">Web 主機建構器。</param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "B2B_API_TEST",
                ["Jwt:Audience"] = "B2B_API_TEST_CLIENT",
                ["Jwt:SecretKey"] = "integration-test-secret-key-with-at-least-32-characters",
                ["Jwt:AccessTokenMinutes"] = "60",
                ["Jwt:RefreshTokenDays"] = "7",
                ["DataAccess:UseFakeRepositories"] = "true",
                ["DataAccess:B2BConn:EnvType"] = "TEST",
                ["DataAccess:B2BConn:SvrType"] = "DEV",
                ["DataAccess:B2BConn:DBType"] = "INET",
                ["DataAccess:B2BConn:AccType"] = "ASI4",
                ["TransactionLog:Enabled"] = "false",
                ["TransactionLog:IncludeRequestBody"] = "false",
                ["TransactionLog:IncludeResponseBody"] = "false",
                ["TransactionLog:TrustForwardedHeaders"] = "false",
                ["TransactionLog:MaxBodyLogLength"] = "10000",
                ["AllowedHosts"] = "localhost"
            });
        });
    }

    /// <summary>
    /// 釋放測試主機並還原測試前的環境變數值。
    /// </summary>
    /// <param name="disposing">是否由 Dispose 流程釋放受控資源。</param>
    protected override void Dispose(bool disposing)
    {
        foreach (var (key, value) in previousEnvironmentValues)
        {
            Environment.SetEnvironmentVariable(key, value);
        }

        base.Dispose(disposing);
    }

    /// <summary>
    /// 設定測試期間使用的環境變數並保存原始值。
    /// </summary>
    /// <param name="key">環境變數名稱。</param>
    /// <param name="value">環境變數值。</param>
    private void SetEnvironment(string key, string value)
    {
        previousEnvironmentValues[key] = Environment.GetEnvironmentVariable(key);
        Environment.SetEnvironmentVariable(key, value);
    }
}
