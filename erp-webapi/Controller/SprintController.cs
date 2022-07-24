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
    
    
    public class SprintController:ControllerBase
    {
        /// <summary>
        ///  Sends data of  closed shipper boxes to the WMS
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetSprintById")]
        [Consumes("application/json")]

        public SprintEditModel GetSprintById(string sprintId, string include = null)
        {
            return SprintManagementService.GetSprintById(sprintId, include);
        }
        
        [HttpGet("GetSprintByProfileId")]
        [Consumes("application/json")]

        public List<SprintEditModel> GetSprintsByProfileId(string profileId)
        {
            return SprintManagementService.GetSprintsByProfileId(profileId);
        }
        
       
        [HttpPost("AddOrUpdateSprint")]
        [Consumes("application/json")]
        public SprintEditModel AddOrUpdateSprint(SprintEditModel sprint)
        {
            return SprintManagementService.AddOrUpdateSprint(sprint);
        }
        
        [HttpDelete("DeleteSprint")]
        [Consumes("application/json")]
        
        public ActionResult<string> DeleteSprint(string sprintId)
        {
            SprintManagementService.DeleteSprint(sprintId);
            return Ok();
        }
        
        [HttpPost("RequestForApproval")]
        [Consumes("application/json")]

        public SprintEditModel RequestForApproval(string sprintId, string userId)
        {
            return SprintManagementService.RequestForApproval(sprintId, userId);
        }

        [HttpPost("ApproveSprint")]
        [Consumes("application/json")]

        public SprintEditModel ApproveSprint(string sprintId, string approverId)
        {
            return SprintManagementService.ApproveSprint(sprintId, approverId);
        }
        
        [HttpPost("RequestForClosure")]
        [Consumes("application/json")]

        public SprintEditModel RequestForClosure(string sprintId, string userId)
        {
            return SprintManagementService.RequestForClosure(sprintId, userId);
        }
        
        [HttpPost("CloseSprint")]
        [Consumes("application/json")]

        public SprintEditModel CloseSprint(string sprintId, string approverId)
        {
            return SprintManagementService.CloseSprint(sprintId, approverId);
        }
        
        [HttpPost("ReviewCompleted")]
        [Consumes("application/json")]
        
        public SprintEditModel ReviewCompleted(string sprintId, string approverId)
        {
            return SprintManagementService.ReviewCompleted(sprintId, approverId);
        }
    }
}