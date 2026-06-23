using System.Globalization;

namespace B2B_Conn;

internal static class PasswordFormatter
{
    private static readonly string[] MonthNames =
    [
        string.Empty,
        "JAN",
        "FEB",
        "MAR",
        "APR",
        "MAY",
        "JUN",
        "JUL",
        "AUG",
        "SEP",
        "OCT",
        "NOV",
        "DEC"
    ];

    public static string Format(string passwordPrefix, int year, int month)
    {
        if (string.IsNullOrEmpty(passwordPrefix) || month is < 1 or > 12)
        {
            return string.Empty;
        }

        return $"{passwordPrefix}_{year.ToString(CultureInfo.InvariantCulture)}{MonthNames[month]}";
    }
}
