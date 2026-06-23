namespace B2B_Conn;

internal static class MonthCredentialSelector
{
    public static string GetMonthlySuffix(int month)
    {
        return month % 2 == 0 ? "2" : "1";
    }
}
