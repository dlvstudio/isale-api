using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;

public class ContactRepository: IContactRepository {

    private readonly IConfiguration _config;

    public ContactRepository(IConfiguration config)
    {
        _config = config;
    }

    public async Task<IEnumerable<Contact>> GetContacts(string userId) {
        using (var db = AppDb)
        {
            string query = @"SELECT
                    *
                FROM `contact`
                WHERE UserId = @UserId";
            await db.Connection.OpenAsync();
            var result = await db.Connection.QueryAsync<Contact>(query, new { UserId = userId });
            if (result != null && result.Any()) {
                var staffIds = result.Select(t => t.StaffId).Distinct();
                string staffsQuery = @"SELECT
                            *
                        FROM `staff`
                        WHERE `UserId` = @UserId
                            AND ID IN @Ids
                    ";
                var staffes = await db.Connection.QueryAsync<Staff>(staffsQuery, new { UserId = userId, Ids = staffIds });
                foreach (var contact in result)
                {
                    if (contact.StaffId == 0) {
                        continue;
                    }
                    var staff = staffes.Where(c => c.Id == contact.StaffId).FirstOrDefault();
                    if (staff == null) {
                        continue;
                    }
                    contact.Staff = staff;
                }
            }
            return result;
        }
    }

    public async Task<Contact> GetById(int contactId, string userId) {
        using (var db = AppDb)
        {
            string query = @"SELECT
                    *
                FROM contact 
                WHERE Id = @Id AND UserId = @UserId
                LIMIT 1";
            await db.Connection.OpenAsync();
            var result = await db.Connection.QueryAsync<Contact>(query, new { Id = contactId, UserId = userId });
            if (result != null && result.Any()) {
                var staffIds = result.Select(t => t.StaffId).Distinct();
                string staffsQuery = @"SELECT
                            *
                        FROM `staff`
                        WHERE `UserId` = @UserId
                            AND ID IN @Ids
                    ";
                var contacts = await db.Connection.QueryAsync<Staff>(staffsQuery, new { UserId = userId, Ids = staffIds });
                foreach (var contact in result)
                {
                    if (contact.StaffId == 0) {
                        continue;
                    }
                    var staff = contacts.Where(c => c.Id == contact.StaffId).FirstOrDefault();
                    if (staff == null) {
                        continue;
                    }
                    contact.Staff = staff;
                }
            }
            return result.FirstOrDefault();
        }
    }

    public async Task<Contact> GetByCode(string contactCode, string userId) {
        using (var db = AppDb)
        {
            string query = @"SELECT
                    *
                FROM contact 
                WHERE `Code` = @Code AND UserId = @UserId
                LIMIT 1";
            await db.Connection.OpenAsync();
            var result = await db.Connection.QueryAsync<Contact>(query, new { Code = contactCode, UserId = userId });
            if (result != null && result.Any()) {
                var staffIds = result.Select(t => t.StaffId).Distinct();
                string staffsQuery = @"SELECT
                            *
                        FROM `staff`
                        WHERE `UserId` = @UserId
                            AND ID IN @Ids
                    ";
                var contacts = await db.Connection.QueryAsync<Staff>(staffsQuery, new { UserId = userId, Ids = staffIds });
                foreach (var contact in result)
                {
                    if (contact.StaffId == 0) {
                        continue;
                    }
                    var staff = contacts.Where(c => c.Id == contact.StaffId).FirstOrDefault();
                    if (staff == null) {
                        continue;
                    }
                    contact.Staff = staff;
                }
            }
            return result.FirstOrDefault();
        }
    }

    public async Task<bool> Remove(int contactId, string userId) {
        using (var db = AppDb)
        {
            string query = @"DELETE
                FROM contact 
                WHERE Id = @Id AND UserId = @UserId";
            await db.Connection.OpenAsync();
            var postResult = await db.Connection.ExecuteAsync(query, new { Id = contactId, UserId = userId });
            return postResult > 0;
        }
    }

    public async Task<int> SaveContact(Contact contact) {
        if (contact == null) {
            return 0;
        }
        using (var db = AppDb)
        {
            string query = string.Empty;
            var isInsert = false;
            if (contact.Id > 0) {
                query = @"
                    UPDATE `contact`
                    SET 
                        avatarUrl = @AvatarUrl
                        ,fullName = @FullName
                        ,mobile = @Mobile
                        ,isImportant = @IsImportant
                        ,gender = @Gender
                        ,`code` = @Code
                        ,`staffId` = @StaffId
                        ,email = @Email
                        ,address = @Address
                        ,dateOfBirth = @DateOfBirth
                        ,lastActive = @LastActive
                        ,lastAction = @LastAction
                        ,point = @Point
                        ,levelId = @LevelId
                    WHERE 
                        id = @Id 
                        AND userId = @UserId
                ";
            } else {
                isInsert = true;
                query = @"INSERT INTO `contact`
                    (   
                        `avatarUrl`,
                        `fullName`,
                        `code`,
                        `mobile`,
                        `isImportant`,
                        `gender`,
                        `email`,
                        `staffId`,
                        `address`,
                        `dateOfBirth`,
                        `lastActive`,
                        `lastAction`,
                        `point`,
                        `levelId`,
                        `userId`
                        )
                    VALUES
                        (
                        @AvatarUrl,
                        @FullName,
                        @Code,
                        @Mobile,
                        @IsImportant,
                        @Gender,
                        @Email,
                        @StaffId,
                        @Address,
                        @DateOfBirth,
                        @LastActive,
                        @LastAction,
                        @Point,
                        @LevelId,
                        @UserId
                        )
                    ;
                    SELECT LAST_INSERT_ID();
                ";
            }
            await db.Connection.OpenAsync();
            if (isInsert) {
                var insertResult = (await db.Connection.QueryAsync<int>(query, contact)).Single();
                return insertResult;
            }
            var postResult = await db.Connection.ExecuteAsync(query, contact);
            return postResult > 0 ? contact.Id : 0;
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