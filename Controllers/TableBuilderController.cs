using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace atakafe_api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TableBuilderController : ControllerBase
    {
        private readonly ISqlService _sqlService;

        private const string Key = "123456ABC999";

        
        public TableBuilderController(
            ISqlService sqlService
        ) {
            _sqlService = sqlService;
        }
        
        // GET /contact/list
        [HttpGet]
        [Route("Columns")]
        public async Task<ActionResult<string>> Columns(string key)
        {
            if (key != Key) {
                return NotFound();
            }
            var tableNames = await _sqlService.Tables();
            string ret = string.Empty;
            foreach (var table in tableNames)
            {
                var columnTypes = await _sqlService.Columns(table);
                var tableVarName = string.Format("tableMapFor{0}", table.ToUpper());
                ret += string.Format("#region Build map for {0}\n", table.ToUpper());
                ret += string.Format("var {0} = new Dictionary<string, string>();\n", tableVarName);
                foreach (var columnType in columnTypes)
                {
                    ret += string.Format("{0}[\"{1}\"] = \"{2}\";\n", tableVarName, columnType.Name.ToLower(), columnType.Type);
                }
                ret += string.Format("tableMap[\"{0}\"] = {1};\n", table.ToUpper(), tableVarName);
                ret += "#endregion\n";
                ret += "\n";
            }
            return ret;
        }
    }
}
