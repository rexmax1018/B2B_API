using System.ComponentModel.DataAnnotations;

namespace B2B.WebApi.Options;

public sealed class TransactionLogOptions
{
    public const string SectionName = "TransactionLog";

    public bool Enabled { get; set; } = true;

    public bool IncludeRequestBody { get; set; } = true;

    public bool IncludeResponseBody { get; set; } = true;

    [Range(100, 1_000_000)]
    public int MaxBodyLogLength { get; set; } = 10000;
}
