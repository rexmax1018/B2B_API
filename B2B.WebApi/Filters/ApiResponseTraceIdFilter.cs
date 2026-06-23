using B2B.WebApi.Model.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace B2B.WebApi.Filters;

/// <summary>
/// 在標準 API 回應中補入本次請求的追蹤編號。
/// </summary>
public sealed class ApiResponseTraceIdFilter : IResultFilter
{
    /// <inheritdoc />
    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is ObjectResult objectResult)
        {
            SetTraceId(objectResult.Value, context.HttpContext.TraceIdentifier);
        }
    }

    /// <inheritdoc />
    public void OnResultExecuted(ResultExecutedContext context)
    {
    }

    private static void SetTraceId(object? value, string traceId)
    {
        var responseType = value?.GetType();

        if (responseType is null ||
            !responseType.IsGenericType ||
            responseType.GetGenericTypeDefinition() != typeof(ApiResponse<>))
        {
            return;
        }

        var traceIdProperty = responseType.GetProperty(nameof(ApiResponse<object>.TraceId));
        var currentValue = traceIdProperty?.GetValue(value) as string;

        if (string.IsNullOrWhiteSpace(currentValue))
        {
            traceIdProperty?.SetValue(value, traceId);
        }
    }
}
