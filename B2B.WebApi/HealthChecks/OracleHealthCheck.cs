using B2B.Dao.Contexts;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace B2B.WebApi.HealthChecks;

/// <summary>
/// 驗證 Oracle 資料庫連線是否可用。
/// </summary>
/// <param name="dbContext">B2B 資料庫內容。</param>
public sealed class OracleHealthCheck(B2BDbContext dbContext) : IHealthCheck
{
    /// <summary>
    /// 執行 Oracle readiness 檢查。
    /// </summary>
    /// <param name="context">健康檢查內容。</param>
    /// <param name="cancellationToken">取消權杖。</param>
    /// <returns>健康檢查結果。</returns>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);

            return canConnect
                ? HealthCheckResult.Healthy("Oracle connection is available.")
                : HealthCheckResult.Unhealthy("Oracle connection is unavailable.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Oracle connection check failed.", ex);
        }
    }
}
