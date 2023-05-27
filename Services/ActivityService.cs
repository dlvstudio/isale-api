using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class ActivityService : IActivityService
{
    private readonly ISqlService _sqlService;

    public ActivityService(
        ISqlService sqlService
    )
    {
        _sqlService = sqlService;
    }

    public async Task<int> Log(string userId, string feature, string action, string note, string session)
    {
        await UpdateOnlineAsync(userId);
        var exists = await _sqlService.ListAsync("user_activity", new Dictionary<string, object>() {
            {"feature", feature},
            {"action", action},
            {"userId", userId},
            {"createdDate", DateTime.Today},
        }, null);
        if (exists != null && exists.Any())
        {
            return 0;
        }
        return await _sqlService.SaveAsync(new Dictionary<string, object>() {
            {"userId", userId},
            {"feature", feature},
            {"action", action},
            {"session", session},
            {"note", note}
        }, "user_activity",
        "Id",
        new List<string>() { "Id", "UserId" },
        null);
    }

    private async Task UpdateOnlineAsync(string userId) {
        var existsOnlines = await _sqlService.ListAsync("user_online", new Dictionary<string, object>() {
            {"userId", userId},
            {"createdDate", DateTime.Today},
        }, null);
        if (existsOnlines == null || !existsOnlines.Any())
        {
            await _sqlService.SaveAsync(new Dictionary<string, object>() {
                {"userId", userId},
                {"createdAt", DateTime.Now},
                {"updatedAt", DateTime.Now},
                {"minutesSpent", 0},
            }, "user_online",
            "Id",
            new List<string>() { "Id", "UserId" }, null);
            await UpdateAgeAsync(userId);
        } else {
            var existsOnline = existsOnlines.FirstOrDefault();
            var startTime = Convert.ToDateTime(existsOnline["createdAt"]);
            var now = DateTime.Now;
            TimeSpan ts = now - startTime;
            var minutes = Convert.ToInt32(ts.TotalMinutes);
            existsOnline["updatedAt"] = now;
            existsOnline["minutesSpent"] = minutes;
            existsOnline.Remove("createdDate");
            await _sqlService.SaveAsync(existsOnline, "user_online", "Id", new List<string>() { "Id", "UserId" }, null);
        }
    }

    private async Task UpdateAgeAsync(string userId) {
        var existsOnlines = await _sqlService.ListAsync("user_age", new Dictionary<string, object>() {
            {"userId", userId},
        }, null);
        if (existsOnlines == null || !existsOnlines.Any())
        {
            await _sqlService.SaveAsync(new Dictionary<string, object>() {
                {"userId", userId},
                {"age", 1},
                {"lastDate", DateTime.Today},
            }, "user_age",
            "Id",
            new List<string>() { "Id", "UserId" }, null);
        } else {
            var existsOnline = existsOnlines.FirstOrDefault();
            var age = Convert.ToInt32(existsOnline["age"]);
            existsOnline["age"] = age + 1;
            existsOnline["lastDate"] = DateTime.Today;
            await _sqlService.SaveAsync(existsOnline, "user_age", "Id", new List<string>() { "Id", "UserId" }, null);
        }
    }
}