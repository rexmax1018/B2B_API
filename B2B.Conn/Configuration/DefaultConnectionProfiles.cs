namespace B2B_Conn;

internal static class DefaultConnectionProfiles
{
    public const string BaseIniPath = @"C:\B2B_Conn\";

    public static IReadOnlyList<B2B_Connection> Create(string? baseIniPath)
    {
        var iniPath = string.IsNullOrWhiteSpace(baseIniPath)
            ? BaseIniPath
            : baseIniPath;

        return
        [
            new() { EnvType = "PROD", SvrType = "WEB", DBType = "INET", AccType = "ASI4", DataSource = "INET.evaair.com", IniPath = iniPath },
            new() { EnvType = "PROD", SvrType = "WEB", DBType = "INET", AccType = "ASI4_CNB2B", DataSource = "INET.evaair.com", IniPath = iniPath },
            new() { EnvType = "PROD", SvrType = "AP", DBType = "INET", AccType = "ASI4", DataSource = "INET.evaair.com", IniPath = iniPath },
            new() { EnvType = "PROD", SvrType = "AP", DBType = "INET", AccType = "ASI4_CNB2B", DataSource = "INET.evaair.com", IniPath = iniPath },
            new() { EnvType = "PROD", SvrType = "BAT", DBType = "INET", AccType = "ASI4", DataSource = "INET.evaair.com", IniPath = iniPath },
            new() { EnvType = "PROD", SvrType = "BAT", DBType = "INET", AccType = "ASI4_CNB2B", DataSource = "INET.evaair.com", IniPath = iniPath },
            new() { EnvType = "TEST", SvrType = "QA", DBType = "INET", AccType = "ASI4", DataSource = "TESTINET.evaair.com", IniPath = iniPath },
            new() { EnvType = "TEST", SvrType = "QA", DBType = "INET", AccType = "ASI4_CNB2B", DataSource = "TESTINET.evaair.com", IniPath = iniPath },
            new() { EnvType = "TEST", SvrType = "DEV", DBType = "INET", AccType = "ASI4", DataSource = "TESTINET.evaair.com", IniPath = iniPath },
            new() { EnvType = "TEST", SvrType = "DEV", DBType = "INET", AccType = "ASI4_CNB2B", DataSource = "TESTINET.evaair.com", IniPath = iniPath }
        ];
    }
}
