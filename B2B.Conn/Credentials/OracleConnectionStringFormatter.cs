namespace B2B_Conn;

internal static class OracleConnectionStringFormatter
{
    public static string Format(Entity_Connection entityInfo)
    {
        if (string.IsNullOrEmpty(entityInfo.DataSource) ||
            string.IsNullOrEmpty(entityInfo.Acc) ||
            string.IsNullOrEmpty(entityInfo.pwd))
        {
            return string.Empty;
        }

        return $"data source={entityInfo.DataSource}; Provider=OraOLEDB.Oracle;OLEDB.NET=True; User ID={entityInfo.Acc}; Password={entityInfo.pwd}; Max Pool Size=100;";
    }
}
