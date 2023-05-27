using System.Collections.Generic;
using System.Threading.Tasks;

public interface IChatGPTService
{
    Task<dynamic> SendMessageAsync(IEnumerable<ChatGPTRoleAndContent> messages);
}