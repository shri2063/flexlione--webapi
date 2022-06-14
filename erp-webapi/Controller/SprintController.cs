using System.Collections.Generic;
using m_sort_server.EditModels;
using m_sort_server.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace m_sort_server.Controller
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [EnableCors("AllowOrigin")]
    [Produces("application/json")]
    
    
    public class SprintController:ControllerBase
    {
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
    }
}