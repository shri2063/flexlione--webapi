using System.Collections.Generic;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace flexli_erp_webapi.Controller

{
    [Route("api/v1/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [EnableCors("AllowOrigin")]
    [Produces("application/json")]
    
    public class SprintReportController : ControllerBase
    {
        /// <summary>
        /// [R]Use to fetch Sprint report for a sprint
        /// [Note]: Sprint No value is not  correct
        /// </summary>
        /// <returns></returns>
        
        [HttpGet("GetSprintReportById")]
        [Consumes("application/json")]
        
       
        public List<SprintReportEditModel> GetSprintReportForSprint(string sprintId)
        {
            return SprintReportManagementService.GetSprintReportForSprint(sprintId);
        }


        /// <summary>
        /// [R]Used For Updating Manager's comments and Approval
        /// Sprint status should not be closed
        /// validate approver id
        ///  sprintReportLineItemId, Manager_comment (editable),Approved(editable)
        /// Approved values ("NoAction", "True", "False")
        /// </summary>
        /// <returns></returns>
        
        [HttpPost("reviewCheckList")]
        [Consumes("application/json")]
        
        public ActionResult<CheckListItemEditModel> ReviewCheckList(SprintReportEditModel sprintReportEditModel, string approverId)
        {
           
            return Ok( SprintReportManagementService.ReviewCheckList(sprintReportEditModel, approverId));
        }
    }
}