using System.Collections.Generic;
using m_sort_server.EditModels;
using m_sort_server.LinkedListModel;
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
    
    public class DependencyController: ControllerBase
    {
        [HttpGet("GetUpstreamDependenciesByTaskId")]
        [Consumes("application/json")]
        
        public List<DependencyEditModel> GetUpstreamDependenciesByTaskId(string taskId,string include = null)
        {
            return DependencyManagementService.GetUpstreamDependenciesByTaskId(taskId,include);
        }
        
        [HttpGet("GetDownstreamDependenciesByTaskId")]
        [Consumes("application/json")]
        
        public List<DependencyEditModel> GetDownstreamDependenciesByTaskId(string taskId,string include = null)
        {
            return DependencyManagementService.GetDownstreamDependenciesByTaskId(taskId,include);
        }
        
        
        /// <summary>
        /// Create or update tasks
        /// Note that if you are creating new taskDetail - what taskDetail Id you mention does not matter
        /// It will assign serially
        /// position after you can keep empty ("")
        /// </summary>
        [HttpPut("CreateOrUpdateDependency")]
        [Consumes("application/json")]
        
        public ActionResult<DependencyEditModel> CreateOrUpdateDependency(DependencyEditModel dependency)
        {
            return DependencyManagementService.CreateOrUpdateDependency(dependency);
        }


        [HttpDelete("DeleteDependency")]
        [Consumes("application/json")]
        
        public ActionResult<string> DeleteDependency(string dependencyId)
        {
            DependencyManagementService.DeleteDependency(dependencyId);
            return Ok();
        }
    }  
    
}