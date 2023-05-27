using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class StaffService : IStaffService
{
    private readonly IStaffRepository _repository;
    private readonly ISqlService _sqlService;

    public StaffService(
        IStaffRepository repository,
        ISqlService sqlService
    ) {
        _repository = repository;
        _sqlService = sqlService;
    }

    public async Task<IEnumerable<Staff>> List(string userId)
    {
        return await _repository.List(userId);
    }

    public async Task<Staff> Get(int staffId, string userId) {
        var post = await _repository.GetById(staffId, userId);
        return post;
    }

    public async Task<bool> Remove(int staffId, string userId) {
        var post = await _repository.Remove(staffId, userId);
        return post;
    }

    public async Task<int> Save(Staff staff) {
        return await _repository.Save(staff);
    }

    public async Task<IEnumerable<Dictionary<string, object>>> CheckPermissions(string userName) {
        var modelStaff = new Dictionary<string, object>();
        modelStaff["UserName"] = userName;
        var staffs = await _sqlService.ListAsync("staff", modelStaff, null);
        if (staffs == null || !staffs.Any()) {
            return staffs;
        }
        var model = new Dictionary<string, object>();
        var userIds = new List<string>();
        foreach (var item in staffs)
        {
            userIds.Add(item["userId"].ToString());
        }
        model.Add("userids", userIds);
        var query = new QueryModelOnSearch();
        query.WhereFieldQuerys = new Dictionary<string, List<string>>();
        query.WhereFieldQuerys["userid"] = new List<string>() {EnumSearchFunctions.IN, "userIds"};
        var shops = await _sqlService.ListAsync("shop", model, query);
        foreach (var item in staffs)
        {
            var shop = shops.FirstOrDefault(s => s.ContainsKey("userId") && s["userId"].ToString() == item["userId"].ToString());
            if (shop == null || !shop.ContainsKey("name")) {
                continue;
            }
            item["shopName"] = shop["name"].ToString();
        }
        return staffs;
    }

    public async Task<Staff> GetByIdOnly(int staffId) {
        var post = await _repository.GetByIdOnly(staffId);
        return post;
    }
}