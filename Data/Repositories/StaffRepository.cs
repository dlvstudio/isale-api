using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;

public class StaffRepository: IStaffRepository {

    private readonly IConfiguration _config;

    public StaffRepository(IConfiguration config)
    {
        _config = config;
    }

    public async Task<IEnumerable<Staff>> List(string userId) {
        using (var db = AppDb)
        {
            string query = @"SELECT
                    *
                FROM `staff` 
                WHERE UserId = @UserId";
            await db.Connection.OpenAsync();
            var result = await db.Connection.QueryAsync<Staff>(query, new { UserId = userId });
            return result;
        }
    }

    public async Task<Staff> GetById(int staffId, string userId) {
        using (var db = AppDb)
        {
            string query = @"SELECT
                    *
                FROM `staff`  
                WHERE Id = @Id AND UserId = @UserId
                LIMIT 1";
            await db.Connection.OpenAsync();
            var result = await db.Connection.QueryAsync<Staff>(query, new { Id = staffId, UserId = userId });
            return result.FirstOrDefault();
        }
    }

    public async Task<Staff> GetByName(string name, string userId) {
        using (var db = AppDb)
        {
            string query = @"SELECT
                    *
                FROM `staff`  
                WHERE Name = @Name AND UserId = @UserId
                LIMIT 1";
            await db.Connection.OpenAsync();
            var result = await db.Connection.QueryAsync<Staff>(query, new { Name = name, UserId = userId });
            return result.FirstOrDefault();
        }
    }

    public async Task<Staff> GetByIdOnly(int staffId) {
        using (var db = AppDb)
        {
            string query = @"SELECT
                    *
                FROM `staff`  
                WHERE Id = @Id
                LIMIT 1";
            await db.Connection.OpenAsync();
            var result = await db.Connection.QueryAsync<Staff>(query, new { Id = staffId });
            return result.FirstOrDefault();
        }
    }

    public async Task<IEnumerable<Staff>> CheckPermissions(string userName) {
        using (var db = AppDb)
        {
            string query = @"SELECT
                    *
                FROM `staff`  
                WHERE UserName = @UserName
                LIMIT 1";
            await db.Connection.OpenAsync();
            var results = await db.Connection.QueryAsync<Staff>(query, new { UserName = userName });
            return results;
        }
    }

    public async Task<bool> Remove(int staffId, string userId) {
        using (var db = AppDb)
        {
            string query = @"DELETE
                FROM `staff`   
                WHERE Id = @Id AND UserId = @UserId";
            await db.Connection.OpenAsync();
            var postResult = await db.Connection.ExecuteAsync(query, new { Id = staffId, UserId = userId });
            return postResult > 0;
        }
    }

    public async Task<int> Save(Staff staff) {
        if (staff == null) {
            return 0;
        }
        using (var db = AppDb)
        {
            string query = string.Empty;
            var isInsert = false;
            if (staff.Id > 0) {
                query = @"
                    UPDATE `staff`   
                    SET 
                        avatarUrl = @AvatarUrl
                        , name = @Name
                        , userName = @UserName
                        , ShopName = @ShopName
                        , HasFullAccess = @HasFullAccess
                        , storeId = @StoreId
                        , shiftId = @ShiftId
                        , CanCreateOrder = @CanCreateOrder
                        , CanUpdateDeleteOrder = @CanUpdateDeleteOrder
                        , CanCreateNewTransaction = @CanCreateNewTransaction
                        , CanUpdateDeleteTransaction = @CanUpdateDeleteTransaction
                        , CanCreateUpdateDebt = @CanCreateUpdateDebt
                        , CanCreateUpdateNote = @CanCreateUpdateNote
                        , CanUpdateDeleteProduct = @CanUpdateDeleteProduct
                        , CanViewProductCostPrice = @CanViewProductCostPrice
                        , CanUpdateProductCostPrice = @CanUpdateProductCostPrice
                        , CanViewAllContacts = @CanViewAllContacts
                        , CanManageContacts = @CanManageContacts
                        , UpdateStatusExceptDone = @UpdateStatusExceptDone
                        , HourLimit = @HourLimit
                        , BlockViewingQuantity = @BlockViewingQuantity
                        , blockEditingOrderPrice = @BlockEditingOrderPrice
                    WHERE 
                        Id = @Id 
                        AND userId = @UserId
                ";
            } else {
                isInsert = true;
                query = @"INSERT INTO `staff`
                    (  
                        `avatarUrl`,
                        `shopName`,
                        `name`,
                        `userName`,
                        `HasFullAccess`,
                        `CanCreateOrder`,
                        `CanUpdateDeleteOrder`,
                        `CanCreateNewTransaction`,
                        `CanUpdateDeleteTransaction`,
                        `CanCreateUpdateDebt`,
                        `CanCreateUpdateNote`,
                        `CanUpdateDeleteProduct`,
                        `CanViewProductCostPrice`,
                        `CanUpdateProductCostPrice`,
                        `CanViewAllContacts`,
                        `CanManageContacts`,
                        `UpdateStatusExceptDone`,
                        `HourLimit`,
                        `storeId`,
                        `shiftId`,
                        `blockViewingQuantity`,
                        `blockEditingOrderPrice`,
                        `userId`
                        )
                    VALUES
                        (
                        @AvatarUrl,
                        @ShopName,
                        @Name,
                        @UserName,
                        @HasFullAccess,
                        @CanCreateOrder,
                        @CanUpdateDeleteOrder,
                        @CanCreateNewTransaction,
                        @CanUpdateDeleteTransaction,
                        @CanCreateUpdateDebt,
                        @CanCreateUpdateNote,
                        @CanUpdateDeleteProduct,
                        @CanViewProductCostPrice,
                        @CanUpdateProductCostPrice,
                        @CanViewAllContacts,
                        @CanManageContacts,
                        @UpdateStatusExceptDone,
                        @HourLimit,
                        @StoreId,
                        @ShiftId,
                        @BlockViewingQuantity,
                        @BlockEditingOrderPrice,
                        @UserId
                        )
                    ;
                    SELECT LAST_INSERT_ID();
                ";
            }
            await db.Connection.OpenAsync();
            if (isInsert) {
                var insertResult = (await db.Connection.QueryAsync<int>(query, staff)).Single();
                return insertResult;
            }
            var postResult = await db.Connection.ExecuteAsync(query, staff);
            return postResult > 0 ? staff.Id : 0;
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