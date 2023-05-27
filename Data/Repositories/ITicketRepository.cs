using System.Threading.Tasks;

public interface ITicketRepository {
    Task<int> Save(Ticket ticket);
}