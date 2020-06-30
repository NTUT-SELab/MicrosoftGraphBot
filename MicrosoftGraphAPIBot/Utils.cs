using Microsoft.Extensions.Configuration;

namespace MicrosoftGraphAPIBot
{
    public static class Utils
    {
        public static string GetDBConnection(IConfiguration configuration)
        {
            string SQLHost = configuration["MSSQL:Host"];
            string SQLPort = configuration["MSSQL:Port"];
            string SQLUser = configuration["MSSQL:User"];
            string SQLPassword = configuration["MSSQL:Password"];
            string SQLDataBase = configuration["MSSQL:DataBase"];
            return string.Format("Data Source={0},{1};Initial Catalog={2};User ID={3};Password={4}", SQLHost, SQLPort, SQLDataBase, SQLUser, SQLPassword);
        }
    }
}
