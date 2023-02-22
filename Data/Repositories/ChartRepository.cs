using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

public class ChartRepository: IChartRepository {

    private readonly IConfiguration _config;

    public ChartRepository(IConfiguration config)
    {
        _config = config;
    }

    public async Task<string> GetTopUsersJson(string alias)
    {
        using (var db = AppDb)
        {
            string query = @"SELECT
                    Users
                FROM top_users 
                WHERE Alias = @Alias
                LIMIT 1";
            await db.Connection.OpenAsync();
            var result = await db.Connection.QueryAsync<string>(query, new { Alias = alias });
            return result.FirstOrDefault();
        }
    }

    public async Task<int> UpdateTopUsers(string alias, List<UserChart> userCharts) {
        using (var db = AppDb)
        {
            string query = @"
                INSERT INTO top_users (Alias, Users) 
                VALUES (@Alias, @UsersJson) 
                ON DUPLICATE KEY 
                UPDATE Users=@UsersJson;";
            await db.Connection.OpenAsync();
            var result = await db.Connection.ExecuteAsync(query, new { 
                Alias = alias, 
                UsersJson = JsonConvert.SerializeObject(userCharts)});
            return result;
        }
    }

    public AppDb AppDb
    {
        get
        {
            return new AppDb(_config.GetConnectionString("DefaultConnection"));
        }
    }
}