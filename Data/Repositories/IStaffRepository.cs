using System.Collections.Generic;
using System.Threading.Tasks;

public interface IStaffRepository {

    Task<IEnumerable<Staff>> List(string userId);

    Task<Staff> GetById(int staffId, string userId);

    Task<Staff> GetByName(string name, string userId);

    Task<bool> Remove(int staffId, string userId);

    Task<int> Save(Staff staff);

    Task<IEnumerable<Staff>> CheckPermissions(string userName);

    Task<Staff> GetByIdOnly(int staffId);
}