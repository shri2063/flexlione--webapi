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

        public string GetSprintsByProfileId(string profileId, Include include)
        {
            return include.ToString();
            // return SprintManagementService.GetSprintsByProfileId(profileId, include.ToString() );
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
    }
}