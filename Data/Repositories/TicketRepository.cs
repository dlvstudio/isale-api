using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;

public class TicketRepository: ITicketRepository {

    private readonly IConfiguration _config;

    public TicketRepository(IConfiguration config)
    {
        _config = config;
    }

    public async Task<int> Save(Ticket ticket) {
        if (ticket == null) {
            return 0;
        }
        using (var db = AppDb)
        {
            string query = string.Empty;
            var isInsert = false;
            if (ticket.Id > 0) {
                query = @"
                    UPDATE `ticket`
                    SET 
                        `content` = @Content
                        ,`email` = @Email
                        ,`subject` = @Subject
                        ,`categoryId` = @CategoryId
                    WHERE 
                        id = @Id 
                        AND userId = @UserId
                ";
            } else {
                isInsert = true;
                query = @"INSERT INTO `ticket`
                    (   
                        `content`,
                        `email`,
                        `subject`,
                        `categoryId`,
                        `userId`
                        )
                    VALUES
                        (
                        @Content,
                        @Email,
                        @Subject,
                        @CategoryId,
                        @UserId
                        )
                    ;
                    SELECT LAST_INSERT_ID();
                ";
            }
            await db.Connection.OpenAsync();
            if (isInsert) {
                var insertResult = (await db.Connection.QueryAsync<int>(query, ticket)).Single();
                return insertResult;
            }
            var postResult = await db.Connection.ExecuteAsync(query, ticket);
            return postResult > 0 ? ticket.Id : 0;
        }
    }

    public AppDb AppDb
    {
        get
        {
            return new AppDb(_config.GetConnectionString("DefaultConnection"));
        }
    }
}