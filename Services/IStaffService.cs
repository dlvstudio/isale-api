using System.Collections.Generic;
using System.Threading.Tasks;

public interface IStaffService
{
    Task<IEnumerable<Staff>> List(string userId);

    Task<Staff> Get(int id, string userId);
    
    Task<Staff> GetByIdOnly(int id);

    Task<bool> Remove(int id, string userId);

    Task<int> Save(Staff staff);

    Task<IEnumerable<Dictionary<string, object>>> CheckPermissions(string userName);
}