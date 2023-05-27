using System.Collections.Generic;
using System.Threading.Tasks;

public interface IChartRepository {
    Task<string> GetTopUsersJson(string alias);
    Task<int> UpdateTopUsers(string alias, List<UserChart> userCharts);
}