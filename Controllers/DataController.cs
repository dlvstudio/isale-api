using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace atakafe_api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class DataController : ControllerBase
    {
        private readonly IChannelQueueService<UserActivity> _queueMessage;
        private readonly ISqlService _sqlService;
        private readonly IStaffService _staffService;
        private readonly ICacheService _cacheService;

        public DataController(
            IChannelQueueService<UserActivity> queueMessage,
            ICacheService cacheService,
            ISqlService sqlService,
            IStaffService staffService
        )
        {
            _queueMessage = queueMessage;
            _cacheService = cacheService;
            _sqlService = sqlService;
            _staffService = staffService;
        }

        [HttpGet]
        public async Task<ActionResult<object>> Get(string table, int id, int? staffId)
        {
            if (string.IsNullOrWhiteSpace(table) || id <= 0)
            {
                return null;
            }
            var userId = User.GetUserId();
            if (staffId.HasValue && staffId.Value > 0)
            {
                var staff = await _staffService.GetByIdOnly(staffId.Value);
                if (staff == null)
                {
                    return null;
                }
                var userName = User.GetEmail();
                if (userName != staff.UserName)
                {
                    return null;
                }
                userId = staff.UserId;
            }
            await _queueMessage.WriteAsync(new UserActivity() {
                UserId = userId,
                Feature = table,
                Action = "detail",
                Note = id.ToString(),
            });
            var key = table + "-" + userId + "-" + id;
            if (_cacheService.GetByIdContains(table, userId, id))
            {
                return _cacheService.GetGetByIdItem(table, userId, id);
            }
            var post = await _sqlService.GetByIdAsync(table, id, userId);
            _cacheService.SetGetByIdItem(table, userId, id, post);
            return post;
        }

        // POST data/save
        [HttpPost]
        [Route("Save")]
        public async Task<ActionResult<int>> Save([FromBody] Dictionary<string, object> model)
        {
            if (model == null)
            {
                return 0;
            }
            var table = model.ContainsKey("table") ? model["table"] : string.Empty;
            if (string.IsNullOrWhiteSpace(table.ToString()))
            {
                return 0;
            }
            var userId = User.GetUserId();
            var staffIdConverted = 0;
            var staffId = model.ContainsKey("staffId") && int.TryParse(model["staffId"].ToString(), out staffIdConverted)
                ? staffIdConverted
                : 0;
            var isShopOwner = true;
            if (staffId > 0)
            {
                var staff = await _staffService.GetByIdOnly(staffId);
                if (staff == null)
                {
                    return null;
                }
                isShopOwner = staff.UserId == userId;
                var userName = User.GetEmail();
                if (userName != staff.UserName && !isShopOwner)
                {
                    return null;
                }
                userId = staff.UserId;
            }
            model["userId"] = userId;
            Dictionary<string, string> exceptionFunctions = BuildExceptionFunctions(table.ToString());
            if (table.ToString().ToUpper() == "CONTACT")
            {
                model.Remove("point");
                model.Remove("levelId");
                model.Remove("buyCount");
            }
            if (table.ToString().ToUpper() == "CONTACT_POINT")
            {
                table = "CONTACT";
            }
            _cacheService.RemoveListEqualItem(table.ToString(), userId);
            if (model.ContainsKey("id"))
            {
                _cacheService.RemoveGetByIdItem(table.ToString(), userId, model["id"].ToString());
            }
            await _queueMessage.WriteAsync(new UserActivity() {
                UserId = userId,
                Feature = table.ToString(),
                Action = "save",
                Note = model.ContainsKey("id") ? model["id"].ToString() : "",
            });
            return await _sqlService.SaveAsync(model, table.ToString(),
                "Id",
                new List<string>() { "Id", "UserId" },
                exceptionFunctions);
        }

        [HttpPost]
        [Route("Remove")]
        public async Task<ActionResult<bool>> Remove(int id, string table, int? staffId)
        {
            if (string.IsNullOrWhiteSpace(table) || id <= 0)
            {
                return false;
            }
            var userId = User.GetUserId();
            if (staffId.HasValue && staffId.Value > 0)
            {
                var staff = await _staffService.GetByIdOnly(staffId.Value);
                if (staff == null)
                {
                    return null;
                }
                var userName = User.GetEmail();
                if (userName != staff.UserName)
                {
                    return null;
                }
                userId = staff.UserId;
            }
            var whereFieldValues = new Dictionary<string, object>();
            whereFieldValues["id"] = id;
            whereFieldValues["userId"] = userId;
            if (table.ToUpper() == "POINT_HISTORY")
            {
                var pointHistory = await _sqlService.GetByIdAsync("point_history", id, userId);
                _cacheService.RemoveListEqualItem("contact", userId);
                await _sqlService.UpdateAsync("contact",
                    new Dictionary<string, object>() { { "id", pointHistory["contactId"] }, { "userId", userId }, { "point", pointHistory["amount"] } },
                    new QueryModelOnSearch()
                    {
                        WhereFieldQuerys = new Dictionary<string, List<string>>() {
                            {"id", new List<string>{EnumSearchFunctions.EQUALS}},
                            {"userid", new List<string>{EnumSearchFunctions.EQUALS}}
                        },
                        UpdateFieldQuerys = new Dictionary<string, List<string>>() {
                            {"point", new List<string>{EnumUpdateFunctions.DECREASE}}
                        },
                    }
                );
            }
            await _queueMessage.WriteAsync(new UserActivity() {
                UserId = userId,
                Feature = table,
                Action = "delete",
                Note = id.ToString(),
            });
            _cacheService.RemoveListEqualItem(table, userId);
            _cacheService.RemoveGetByIdItem(table.ToString(), userId, id.ToString());
            var post = await _sqlService.RemoveAsync(table, whereFieldValues);
            return post;
        }

        // GET /data/list
        [HttpGet]
        [Route("List")]
        public async Task<IEnumerable<Dictionary<string, object>>> List(string table, int? staffId, int? filterByStaff, string dateFrom, string dateTo, int? status)
        {
            var userId = User.GetUserId();
            bool hasFullAccess = false;
            bool canViewAll = false;
            var isShopOwner = true;
            if (staffId.HasValue && staffId.Value > 0)
            {
                var staff = await _staffService.GetByIdOnly(staffId.Value);
                if (staff == null)
                {
                    return null;
                }
                hasFullAccess = staff.HasFullAccess;
                if (table.ToLower() == "contact")
                {
                    if (staff.CanViewAllContacts)
                    {
                        canViewAll = true;
                    }
                }
                isShopOwner = staff.UserId == userId;
                var userName = User.GetEmail();
                if (userName != staff.UserName && !isShopOwner)
                {
                    return null;
                }
                userId = staff.UserId;
            }
            var staffRealId = staffId.HasValue && !hasFullAccess && !isShopOwner && !canViewAll
                ? staffId.Value
                : ((staffId.HasValue && hasFullAccess || isShopOwner || canViewAll) && filterByStaff.HasValue)
                    ? filterByStaff.Value
                    : 0;
            var model = new Dictionary<string, object>();
            model["userId"] = userId;
            if (staffRealId > 0)
            {
                model["staffId"] = staffRealId;
            }
            var queryModel = QueryModelOnSearch.CreateEqualsFromModel(model);
            if (!string.IsNullOrEmpty(dateFrom) || !string.IsNullOrEmpty(dateTo))
            {
                var dateFromConvert = !string.IsNullOrEmpty(dateFrom)
                    ? DateTime.ParseExact(dateFrom, new string[] { "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd" }, null)
                    : (DateTime?)null;
                var dateToConvert = !string.IsNullOrEmpty(dateTo)
                    ? DateTime.ParseExact(dateTo, new string[] { "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd" }, null)
                    : (DateTime?)null;
                if (dateFromConvert.HasValue && !dateToConvert.HasValue)
                {
                    model["dateFrom"] = dateFromConvert.Value.Date.ToString("yyyy-MM-dd");
                    queryModel.WhereFieldQuerys["createdAt"] = new List<string> { EnumSearchFunctions.BIGGER_OR_EQUALS_THAN, "dateFrom" };
                }
                if (dateToConvert.HasValue && !dateFromConvert.HasValue)
                {
                    model["dateTo"] = dateToConvert.Value.Date.AddDays(1).ToString("yyyy-MM-dd");
                    queryModel.WhereFieldQuerys["createdAt"] = new List<string> { EnumSearchFunctions.SMALLER_OR_EQUALS_THAN, "dateTo" };
                }
                if (dateToConvert.HasValue && dateFromConvert.HasValue)
                {
                    model["dateFrom"] = dateFromConvert.Value.Date.ToString("yyyy-MM-dd");
                    model["dateTo"] = dateToConvert.Value.Date.AddDays(1).ToString("yyyy-MM-dd");
                    queryModel.WhereFieldQuerys["createdAt"] = new List<string> { EnumSearchFunctions.BETWEENS, "dateFrom", "dateTo" };
                }
            }
            if (status.HasValue)
            {
                model["status"] = status.Value;
                queryModel.WhereFieldQuerys["status"] = new List<string> { EnumSearchFunctions.EQUALS, "status" };
            }
            await _queueMessage.WriteAsync(new UserActivity() {
                UserId = userId,
                Feature = table,
                Action = "list",
                Note = "list",
            });
            var results = await _sqlService.ListAsync(table, model, queryModel);
            return results;
        }

        // GET /data/ListEqual
        [HttpPost]
        [Route("ListEqual")]
        public async Task<IEnumerable<Dictionary<string, object>>> ListEqual([FromBody] Dictionary<string, object> model)
        {
            if (model == null)
            {
                return null;
            }
            var table = model.ContainsKey("table") ? model["table"] as string : string.Empty;
            if (string.IsNullOrWhiteSpace(table.ToString()))
            {
                return null;
            }
            var userId = User.GetUserId();
            var isShopOwner = true;
            var canViewAll = false;
            var hasFullAccess = false;
            var staffIdConverted = 0;
            var staffId = model.ContainsKey("staffId") && int.TryParse(model["staffId"].ToString(), out staffIdConverted)
                ? staffIdConverted
                : 0;
            if (staffId > 0)
            {
                var staff = await _staffService.GetByIdOnly(staffId);
                if (staff == null)
                {
                    return null;
                }
                hasFullAccess = staff.HasFullAccess;
                if (table.ToLower() == "contact")
                {
                    if (staff.CanViewAllContacts)
                    {
                        canViewAll = true;
                    }
                }
                isShopOwner = staff.UserId == userId;
                var userName = User.GetEmail();
                if (userName != staff.UserName && !isShopOwner)
                {
                    return null;
                }
                userId = staff.UserId;
            }
            var filterByStaffConverted = 0;
            var filterByStaff = model.ContainsKey("filterByStaff") && int.TryParse(model["filterByStaff"].ToString(), out filterByStaffConverted)
                ? filterByStaffConverted
                : 0;
            var staffRealId = staffId > 0 && !hasFullAccess && !isShopOwner && !canViewAll
                ? staffId
                : ((staffId > 0 && hasFullAccess || isShopOwner || canViewAll) && filterByStaffConverted > 0)
                    ? filterByStaffConverted
                    : 0;
            if (staffRealId > 0)
            {
                model["staffId"] = staffRealId;
            }
            model["userId"] = userId;
            await _queueMessage.WriteAsync(new UserActivity() {
                UserId = userId,
                Feature = table,
                Action = "list",
                Note = "equal",
            });
            var key = string.Join("-", model);
            if (_cacheService.ListEqualContains(table, userId))
            {
                var tableCache = _cacheService.GetListEqualItem(table, userId);
                if (tableCache.ContainsKey(key))
                {
                    return tableCache[key];
                }
            }
            Dictionary<string, string> exceptionFunctions = BuildExceptionFunctions(table.ToString());
            var data = await _sqlService.ListAsync(table.ToString(), model, null);
            if (!_cacheService.ListEqualContains(table, userId))
            {
                var tableCache = new Dictionary<string, IEnumerable<Dictionary<string, object>>>();
                tableCache.Add(key, data);
                _cacheService.SetListEqualItem(table, userId, tableCache);
            }
            else
            {
                var tableCache = _cacheService.GetListEqualItem(table, userId);
                if (tableCache.ContainsKey(key))
                {
                    tableCache[key] = data;
                }
                _cacheService.SetListEqualItem(table, userId, tableCache);
            }
            return data;
        }

        // GET /data/ListMultiEquals
        [HttpPost]
        [Route("ListMultiEquals")]
        public async Task<IEnumerable<Dictionary<string, object>>> ListMultiEquals([FromBody] Dictionary<string, object> model)
        {
            if (model == null)
            {
                return null;
            }
            var table = model.ContainsKey("table") ? model["table"] as string : string.Empty;
            if (string.IsNullOrWhiteSpace(table.ToString()))
            {
                return null;
            }
            var userId = User.GetUserId();
            var isShopOwner = true;
            var canViewAll = false;
            var hasFullAccess = false;
            var staffIdConverted = 0;
            var staffId = model.ContainsKey("staffId") && int.TryParse(model["staffId"].ToString(), out staffIdConverted)
                ? staffIdConverted
                : 0;
            if (staffId > 0)
            {
                var staff = await _staffService.GetByIdOnly(staffId);
                if (staff == null)
                {
                    return null;
                }
                hasFullAccess = staff.HasFullAccess;
                if (table.ToLower() == "contact")
                {
                    if (staff.CanViewAllContacts)
                    {
                        canViewAll = true;
                    }
                }
                isShopOwner = staff.UserId == userId;
                var userName = User.GetEmail();
                if (userName != staff.UserName && !isShopOwner)
                {
                    return null;
                }
                userId = staff.UserId;
            }
            var filterByStaffConverted = 0;
            var filterByStaff = model.ContainsKey("filterByStaff") && int.TryParse(model["filterByStaff"].ToString(), out filterByStaffConverted)
                ? filterByStaffConverted
                : 0;
            var staffRealId = staffId > 0 && !hasFullAccess && !isShopOwner && !canViewAll
                ? staffId
                : ((staffId > 0 && hasFullAccess || isShopOwner || canViewAll) && filterByStaffConverted > 0)
                    ? filterByStaffConverted
                    : 0;
            if (staffRealId > 0)
            {
                model["staffId"] = staffRealId;
            }
            model["userId"] = userId;
            await _queueMessage.WriteAsync(new UserActivity() {
                UserId = userId,
                Feature = table,
                Action = "list",
                Note = "multi-equal",
            });
            var key = string.Join("-", model);
            if (_cacheService.ListEqualContains(table, userId))
            {
                var tableCache = _cacheService.GetListEqualItem(table, userId);
                if (tableCache.ContainsKey(key))
                {
                    return tableCache[key];
                }
            }
            var query = new QueryModelOnSearch() { WhereFieldQuerys = new Dictionary<string, List<string>>() };
            var newModel = new Dictionary<string, object>();
            foreach (var keyDic in model.Keys)
            {
                if (keyDic == "table" || keyDic == "userId")
                {
                    newModel.Add(keyDic, model[keyDic]);
                    query.WhereFieldQuerys.Add(keyDic, new List<string>() { EnumSearchFunctions.EQUALS });
                    continue;
                }
                var obj = model[keyDic];
                IEnumerable<JToken> l = null;
                try
                {
                    l = (IEnumerable<JToken>)model[keyDic];
                }
                catch (System.Exception)
                {
                    l = null;
                }
                if (l != null)
                {
                    var newL = new List<string>();
                    foreach (var item in l)
                    {
                        newL.Add(item.Value<string>());
                    }
                    if (keyDic != "code") {
                        newModel[keyDic] = newL;
                    } else {
                        newModel["codes"] = newL;
                    }
                    query.WhereFieldQuerys.Add(keyDic, new List<string>() { EnumSearchFunctions.IN, keyDic == "code" ? "codes" : keyDic });
                    continue;
                }
                newModel.Add(keyDic, model[keyDic]);
                query.WhereFieldQuerys.Add(keyDic, new List<string>() { EnumSearchFunctions.EQUALS });
            }
            var data = await _sqlService.ListAsync(table.ToString(), newModel, query);
            if (!_cacheService.ListEqualContains(table, userId))
            {
                var tableCache = new Dictionary<string, IEnumerable<Dictionary<string, object>>>();
                tableCache.Add(key, data);
                _cacheService.SetListEqualItem(table, userId, tableCache);
            }
            else
            {
                var tableCache = _cacheService.GetListEqualItem(table, userId);
                if (tableCache.ContainsKey(key))
                {
                    tableCache[key] = data;
                }
                _cacheService.SetListEqualItem(table, userId, tableCache);
            }
            return data;
        }

        // GET /data/CountEqual
        [HttpPost]
        [Route("CountEqual")]
        public async Task<int> CountEqual([FromBody] Dictionary<string, object> model)
        {
            if (model == null)
            {
                return 0;
            }
            var table = model.ContainsKey("table") ? model["table"] as string : string.Empty;
            if (string.IsNullOrWhiteSpace(table.ToString()))
            {
                return 0;
            }
            var userId = User.GetUserId();
            var isShopOwner = true;
            var canViewAll = false;
            var hasFullAccess = false;
            var staffIdConverted = 0;
            var staffId = model.ContainsKey("staffId") && int.TryParse(model["staffId"].ToString(), out staffIdConverted)
                ? staffIdConverted
                : 0;
            if (staffId > 0)
            {
                var staff = await _staffService.GetByIdOnly(staffId);
                if (staff == null)
                {
                    return 0;
                }
                hasFullAccess = staff.HasFullAccess;
                if (table.ToLower() == "contact")
                {
                    if (staff.CanViewAllContacts)
                    {
                        canViewAll = true;
                    }
                }
                isShopOwner = staff.UserId == userId;
                var userName = User.GetEmail();
                if (userName != staff.UserName && !isShopOwner)
                {
                    return 0;
                }
                userId = staff.UserId;
            }
            var filterByStaffConverted = 0;
            var filterByStaff = model.ContainsKey("filterByStaff") && int.TryParse(model["filterByStaff"].ToString(), out filterByStaffConverted)
                ? filterByStaffConverted
                : 0;
            var staffRealId = staffId > 0 && !hasFullAccess && !isShopOwner && !canViewAll
                ? staffId
                : ((staffId > 0 && hasFullAccess || isShopOwner || canViewAll) && filterByStaffConverted > 0)
                    ? filterByStaffConverted
                    : 0;
            if (staffRealId > 0)
            {
                model["staffId"] = staffRealId;
            }
            model["userId"] = userId;
            Dictionary<string, string> exceptionFunctions = BuildExceptionFunctions(table.ToString());
            return await _sqlService.CountAsync(table.ToString(), model, null);
        }

        private Dictionary<string, string> BuildExceptionFunctions(string table)
        {
            Dictionary<string, string> exceptionFunctions = null;
            var tableUpper = table.ToUpper();
            if (tableUpper == "CONTACT")
            {
                exceptionFunctions = new Dictionary<string, string>();
                exceptionFunctions["LastActive"] = EnumExceptionFunctions.EMPTY_THEN_NOW;
                exceptionFunctions["StaffId"] = EnumExceptionFunctions.EMPTY_THEN_ZERO;
                exceptionFunctions["IsImportant"] = EnumExceptionFunctions.EMPTY_THEN_FALSE;
            }
            if (tableUpper == "NOTE")
            {
                exceptionFunctions = new Dictionary<string, string>();
                exceptionFunctions["createdAt"] = EnumExceptionFunctions.EMPTY_THEN_NOW;
                exceptionFunctions["modifiedAt"] = EnumExceptionFunctions.EMPTY_THEN_NOW;
            }
            return BuildExceptionFunctionsWithLower(exceptionFunctions);
        }

        private Dictionary<string, string> BuildExceptionFunctionsWithLower(Dictionary<string, string> exceptionFunctions)
        {
            if (exceptionFunctions == null)
            {
                return exceptionFunctions;
            }
            var newDic = new Dictionary<string, string>();
            var keys = exceptionFunctions.Keys.AsList();
            foreach (var key in keys)
            {
                newDic[key.ToLower()] = exceptionFunctions[key];
            }
            return newDic;
        }
    }
}
