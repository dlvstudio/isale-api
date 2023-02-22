using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IReceivedNoteRepository {

    Task<IEnumerable<ReceivedNote>> List(string userId, DateTime dateFrom, DateTime dateTo, int contactId, int staffId, int storeId);

    Task<ReceivedNote> GetById(int noteId, string userId);

    Task<bool> Remove(int noteId, string userId);

    Task<int> Save(ReceivedNote trade);
}