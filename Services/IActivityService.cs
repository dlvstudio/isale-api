using System.Threading.Tasks;

public interface IActivityService {

    Task<int> Log(string userId, string feature, string action, string note, string session);

}