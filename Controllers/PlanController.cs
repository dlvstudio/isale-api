using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace atakafe_api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class PlanController : ControllerBase
    {
        private readonly ISqlService _sqlService;
        private readonly IStaffService _staffService;

        public PlanController(
            ISqlService sqlService,
            IStaffService staffService
        )
        {
            _sqlService = sqlService;
            _staffService = staffService;
        }

        [HttpGet]
        [Route("GetCurrentPlan")]
        public async Task<ActionResult<object>> GetCurrentPlan(int? staffId)
        {
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
            var model = new Dictionary<string, object>{
                {"userId", userId},
                {"subscriptionType", "PRO"},
            };
            var query = new QueryModelOnSearch() {
                WhereFieldQuerys = new Dictionary<string, List<string>> {
                    {"userId", new List<string>{EnumSearchFunctions.EQUALS}},
                    {"startDate", new List<string>{EnumSearchFunctions.SMALLER_THAN_TODAY}},
                    {"endDate", new List<string>{EnumSearchFunctions.BIGGER_THAN_TODAY}},
                }
            };
            var subscriptions = await _sqlService.ListAsync("subscription", model, query);
            if (subscriptions != null && subscriptions.Any()) {
                return subscriptions.First();
            }
            return null;
        }

        [HttpGet]
        [Route("CheckProduct")]
        public async Task<ActionResult<int>> CheckProduct(int? staffId)
        {
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
            var model = new Dictionary<string, object>{
                {"userId", userId},
            };
            var query = new QueryModelOnSearch() {
                WhereFieldQuerys = new Dictionary<string, List<string>> {
                    {"userId", new List<string>{EnumSearchFunctions.EQUALS}},
                }
            };
            var result = await _sqlService.CountAsync("product", model, query);
            return result;
        }

        [HttpGet]
        [Route("CheckOrder")]
        public async Task<ActionResult<int>> CheckOrder(int? staffId)
        {
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
            var model = new Dictionary<string, object>{
                {"userId", userId},
            };
            var query = new QueryModelOnSearch() {
                WhereFieldQuerys = new Dictionary<string, List<string>> {
                    {"userId", new List<string>{EnumSearchFunctions.EQUALS}},
                    {"createdAt", new List<string>{EnumSearchFunctions.IS_TODAY}}
                }
            };
            var result = await _sqlService.CountAsync("order", model, query);
            return result;
        }
    }
}
