using B2B.WebApi.Options;
using Microsoft.AspNetCore.Builder;

namespace B2B.WebApi.Extensions;

internal static class SecurityConfigurationValidator
{
    private static readonly string PlaceholderJwtSecret =
        string.Concat("PLEASE_CHANGE", "_THIS_SECRET_KEY", "_TO_AT_LEAST_32_CHARS");

    public static void Validate(WebApplication app)
    {
        ValidateJwtSecret(app.Configuration[$"{JwtOptions.SectionName}:SecretKey"]);

        if (app.Environment.IsDevelopment())
        {
            return;
        }

        RejectEnabled(app.Configuration, "DataAccess:UseFakeRepositories", "Fake repositories must not be enabled outside Development.");
        RejectEnabled(app.Configuration, "TransactionLog:IncludeRequestBody", "Request body logging must not be enabled outside Development.");
        RejectEnabled(app.Configuration, "TransactionLog:IncludeResponseBody", "Response body logging must not be enabled outside Development.");

        var allowedHosts = app.Configuration["AllowedHosts"];

        if (string.IsNullOrWhiteSpace(allowedHosts) || string.Equals(allowedHosts, "*", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("AllowedHosts must be explicitly configured outside Development.");
        }
    }

    public static void ValidateJwtSecret(string? secret)
    {
        if (string.IsNullOrWhiteSpace(secret))
        {
            throw new InvalidOperationException("Jwt:SecretKey must be supplied from a secure configuration source.");
        }

        if (string.Equals(secret, PlaceholderJwtSecret, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Jwt:SecretKey still uses the placeholder value.");
        }
    }

    private static void RejectEnabled(IConfiguration configuration, string key, string message)
    {
        if (bool.TryParse(configuration[key], out var enabled) && enabled)
        {
            throw new InvalidOperationException(message);
        }
    }
}
