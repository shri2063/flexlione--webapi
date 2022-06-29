using System.Collections.Generic;
using System.Threading.Tasks;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Services;
using flexli_erp_webapi.DataModels;
using Microsoft.AspNetCore.Mvc;
using flexli_erp_webapi.Models;
using Microsoft.AspNetCore.Cors;


namespace flexli_erp_webapi.Controller
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [EnableCors("AllowOrigin")]
    [Produces("application/json")]
    
    public class CheckListController : ControllerBase
    {
        
        [HttpGet("GetCheckList")]
        [Consumes("application/json")]
        
        public List<CheckListItemEditModel> GetCheckList([FromQuery] string include,string taskId = null)
        {
            return CheckListManagementService.GetCheckList(taskId,include);
        }
        
        [HttpPut("CreateOrUpdateCheckListItem")]
        [Consumes("application/json")]
        
        public CheckListItemEditModel CreateOrUpdateCheckListItem(CheckListItemEditModel checkListItemItem)
        {
            return CheckListManagementService.CreateOrUpdateCheckListItem(checkListItemItem);
        }
        
        [HttpDelete("DeleteCheckListItem")]
        [Consumes("application/json")]
        
        public ActionResult<string> DeleteCheckListItem(string checkListItemId)
        {
            CheckListManagementService.DeleteCheckListItem(checkListItemId);
            return Ok();
        }
    }
}