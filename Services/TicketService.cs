using System.Threading.Tasks;

public class TicketService : ITicketService
{
    private readonly ITicketRepository _repository;

    public TicketService(
        ITicketRepository repository
    ) {
        _repository = repository;
    }

    public async Task<int> Save(Ticket ticket) {
        return await _repository.Save(ticket);
    }
}