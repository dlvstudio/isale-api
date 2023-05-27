using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;

public class AccountRepository: IAccountRepository {

    private readonly IConfiguration _config;

    public AccountRepository(IConfiguration config)
    {
        _config = config;
    }

    public async Task<IEnumerable<Account>> GetAccounts(string userId) {
        using (var db = AppDb)
        {
            string query = @"SELECT
                    *
                FROM money_account 
                WHERE UserId = @UserId
                ";
            await db.Connection.OpenAsync();
            var result = await db.Connection.QueryAsync<Account>(query, new { UserId = userId });
            return result;
        }
    }

    public async Task<Account> GetById(int accountId, string userId) {
        using (var db = AppDb)
        {
            string query = @"SELECT
                    *
                FROM money_account 
                WHERE Id = @Id AND UserId = @UserId
                LIMIT 1";
            await db.Connection.OpenAsync();
            var results = await db.Connection.QueryAsync<Account>(query, new { Id = accountId, UserId = userId });
            var result = results.FirstOrDefault();
            return result;
        }
    }

    public async Task<Account> GetDefault(string userId) {
        using (var db = AppDb)
        {
            string query = @"SELECT
                    *
                FROM money_account 
                WHERE UserId = @UserId AND IsDefault = 1
                LIMIT 1";
            await db.Connection.OpenAsync();
            var results = await db.Connection.QueryAsync<Account>(query, new { UserId = userId });
            var result = results.FirstOrDefault();
            return result;
        }
    }

    public async Task<Account> GetStoreDefault(string userId, int storeId) {
        using (var db = AppDb)
        {
            string query = @"SELECT
                    *
                FROM money_account 
                WHERE UserId = @UserId AND DefaultStoreId = @StoreId
                LIMIT 1";
            await db.Connection.OpenAsync();
            var results = await db.Connection.QueryAsync<Account>(query, new { UserId = userId, StoreId = storeId });
            var result = results.FirstOrDefault();
            return result;
        }
    }

    public async Task<bool> Remove(int accountId, string userId) {
        using (var db = AppDb)
        {
            string query = @"DELETE
                FROM money_account 
                WHERE Id = @Id AND UserId = @UserId";
            await db.Connection.OpenAsync();
            var postResult = await db.Connection.ExecuteAsync(query, new { Id = accountId, UserId = userId });
            return postResult > 0;
        }
    }

    public async Task<int> SaveAccount(Account account) {
        if (account == null) {
            return 0;
        }
        using (var db = AppDb)
        {
            string query = string.Empty;
            var isInsert = false;
            if (account.Id > 0) {
                account.ModifiedAt = DateTime.Now;
                query = @"
                    UPDATE `money_account`
                    SET 
                        accountName = @AccountName
                        ,total = @Total
                        ,bankAccountName = @BankAccountName
                        ,bankName = @BankName
                        ,bankNumber = @BankNumber
                        ,modifiedAt = @ModifiedAt
                        ,isDefault = @IsDefault
                        ,defaultStoreId = @DefaultStoreId
                    WHERE 
                        id = @Id 
                        AND userId = @UserId
                ";
            } else {
                isInsert = true;
                account.CreatedAt = DateTime.Now;
                account.ModifiedAt = account.CreatedAt;
                query = @"INSERT INTO `money_account`
                    (   
                        `accountName`,
                        `bankAccountName`,
                        `bankName`,
                        `bankNumber`,
                        `total`,
                        `createdAt`,
                        `modifiedAt`,
                        `isDefault`,
                        `defaultStoreId`,
                        `userId`
                        )
                    VALUES
                        (
                        @AccountName,
                        @BankAccountName,
                        @BankName,
                        @BankNumber,
                        @Total,
                        @CreatedAt,
                        @ModifiedAt,
                        @IsDefault,
                        @DefaultStoreId,
                        @UserId
                        )
                    ;
                    SELECT LAST_INSERT_ID();
                ";
            }
            await db.Connection.OpenAsync();
            if (isInsert) {
                var insertResult = (await db.Connection.QueryAsync<int>(query, account)).Single();
                return insertResult;
            }
            var postResult = await db.Connection.ExecuteAsync(query, account);
            return postResult > 0 ? account.Id : 0;
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