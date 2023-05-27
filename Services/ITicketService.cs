using System.Threading.Tasks;

public interface ITicketService
{
    Task<int> Save(Ticket ticket);
}