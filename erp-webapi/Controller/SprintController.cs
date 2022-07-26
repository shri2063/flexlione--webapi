using System;
using System.Collections.Generic;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace flexli_erp_webapi.Controller
{
    public  enum Include {PlannedTasks, UnPlannedTasks}
    
    [Route("api/v1/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [EnableCors("AllowOrigin")]
    [Produces("application/json")]
    
   
    public class SprintController:ControllerBase
    {
       
        
        /// <summary>
        ///  [R]Include - plannedTask,unPlannedTask
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetSprintById")]
        [Consumes("application/json")]

        public SprintEditModel GetSprintById(string sprintId, List<string> include = null)
        {
            return SprintManagementService.GetSprintById(sprintId, include);
        }
        
        /// <summary>
        ///  ///  [R]Include - plannedTask,unPlannedTask
        /// </summary>
        /// <returns></returns>
        
        [HttpGet("GetSprintByProfileId")]
        [Consumes("application/json")]

        public List<SprintEditModel> GetSprintsByProfileId(string profileId, List<string> include)
        {
            return SprintManagementService.GetSprintsByProfileId(profileId, include);
        }
        
        /// <summary>
        /// [R][Check]: Previous all sprints closed in case of new sprint
        /// [Check]: Sprint is in planning state in case of already created sprint
        /// </summary>
        /// <returns></returns>
        [HttpPost("AddOrUpdateSprint")]
        [Consumes("application/json")]
        public SprintEditModel AddOrUpdateSprint(SprintEditModel sprint)
        {
            return SprintManagementService.AddOrUpdateSprint(sprint);
        }
        
        /// <summary>
        /// [R][Check]: Sprint is in planning state 
        /// </summary>
        /// <returns></returns>
        [HttpDelete("DeleteSprint")]
        [Consumes("application/json")]
        
        public ActionResult<string> DeleteSprint(string sprintId)
        {
            SprintManagementService.DeleteSprint(sprintId);
            return Ok();
        }
        
        /// <summary>
        /// [R][Check]: Sprint State is Planning
        /// [Available States]:  Planning, RequestForApproval, Approved, RequestForClosure, Closed, Reviewed
        /// [Action]: No Checklist item can be added/edited/deleted once Status = requestForApproval
        /// </summary>
        /// <returns></returns>
        [HttpPost("RequestForApproval")]
        [Consumes("application/json")]

        public SprintEditModel RequestForApproval(string sprintId, string userId)
        {
            return SprintManagementService.RequestForApproval(sprintId, userId);
        }
        
        /// <summary>
        /// [R][Check]: Sprint is in RequestForApproval , approver id is valid
        /// [Action] : Sprint report line items will be generated once approved
        /// </summary>
        /// <returns></returns>
        [HttpPost("ApproveSprint")]
        [Consumes("application/json")]

        public SprintEditModel ApproveSprint(string sprintId, string approverId)
        {
            return SprintManagementService.ApproveSprint(sprintId, approverId);
        }
        /// <summary>
        /// [R][Check]: Sprint is in Approved state
        /// [Action] : Result and user comment cannot be edited once RequestForClosure
        /// </summary>
        /// <returns></returns>
        [HttpPost("RequestForClosure")]
        [Consumes("application/json")]

       
        public SprintEditModel RequestForClosure(string sprintId, string userId)
        {
            return SprintManagementService.RequestForClosure(sprintId, userId);
        }
        
        /// <summary>
        /// [R][Check]: Sprint is in RequestForClosure, approver Id is valid
        /// [Action] : provisional Score will be created once closed
        /// [Action]: All tasks will be unlinked from the sprint
        /// </summary>
        /// <returns></returns>
        [HttpPost("CloseSprint")]
        [Consumes("application/json")]

        public SprintEditModel CloseSprint(string sprintId, string approverId)
        {
            return SprintManagementService.CloseSprint(sprintId, approverId);
        }
        
        /// <summary>
        /// [R][Check]: Sprint is in Closed state , approver Id is valid
        /// [Action] No manager comment  and checklist item score can be updated once review completed
        /// </summary>
        /// <returns></returns>
        [HttpPost("ReviewCompleted")]
        [Consumes("application/json")]
        
        
        public SprintEditModel ReviewCompleted(string sprintId, string approverId)
        {
            return SprintManagementService.ReviewCompleted(sprintId, approverId);
        }
    }
}