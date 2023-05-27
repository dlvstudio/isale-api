using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ReceivedNoteService : IReceivedNoteService
{
    private readonly IReceivedNoteRepository _repository;
    private readonly IProductRepository _productRepository;

    public ReceivedNoteService(
        IReceivedNoteRepository repository,
        IProductRepository productRepository
    ) {
        _repository = repository;
        _productRepository = productRepository;
    }

    public async Task<IEnumerable<ReceivedNote>> List(string userId, DateTime? dateFrom, DateTime? dateTo, int contactId, int staffId, int storeId)
    {
        return await _repository.List(userId, 
            dateFrom.HasValue ? dateFrom.Value : DateTime.Now.AddMonths(-6), 
            dateTo.HasValue ? dateTo.Value : DateTime.Now.AddMonths(3),
            contactId, staffId, storeId);
    }

    public async Task<ReceivedNote> Get(int id, string userId) {
        var post = await _repository.GetById(id, userId);
        return post;
    }

    public async Task<bool> Remove(int id, string userId) {
        var post = await _repository.Remove(id, userId);
        return post;
    }

    public async Task<int> Save(ReceivedNote note) {
        return await _repository.Save(note);
    }
}