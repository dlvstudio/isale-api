using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IReceivedNoteService
{
    Task<IEnumerable<ReceivedNote>> List(string userId, DateTime? dateFrom, DateTime? dateTo, int contactId, int staffId, int storeId);

    Task<ReceivedNote> Get(int id, string userId);

    Task<bool> Remove(int id, string userId);

    Task<int> Save(ReceivedNote order);
}