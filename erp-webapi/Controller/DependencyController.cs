using System.Collections.Generic;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Services;
using flexli_erp_webapi.LinkedListModel;
using mflexli_erp_webapi.Repository.Interfaces;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace flexli_erp_webapi.Controller
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [EnableCors("AllowOrigin")]
    [Produces("application/json")]
    
    public class DependencyController: ControllerBase
    {
        
        
        private readonly IDependencyRepository _dependencyRepository;
        private readonly DependencyManagementService _dependencyManagementService;

        public DependencyController(IDependencyRepository dependencyRepository, DependencyManagementService dependencyManagementService)
        {
            _dependencyRepository = dependencyRepository;
            _dependencyManagementService = dependencyManagementService;
        }
        
        [HttpGet("GetUpstreamDependenciesByTaskId")]
        [Consumes("application/json")]
        
        public List<DependencyEditModel> GetUpstreamDependenciesByTaskId(string taskId, int? pageIndex = null, int? pageSize = null)
        {
            return _dependencyManagementService.GetUpstreamDependenciesByTaskId(taskId, pageIndex, pageSize);
        }
        
        [HttpGet("GetDownstreamDependenciesByTaskId")]
        [Consumes("application/json")]
        
        public List<DependencyEditModel> GetDownstreamDependenciesByTaskId(string taskId,string include = null, int? pageIndex = null, int? pageSize = null)
        {
            return _dependencyManagementService.GetDownstreamDependenciesByTaskId(taskId, pageIndex, pageSize);
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
            return _dependencyManagementService.CreateOrUpdateDependency(dependency);
        }


        [HttpDelete("DeleteDependency")]
        [Consumes("application/json")]
        
        public ActionResult<string> DeleteDependency(string dependencyId)
        {
            _dependencyManagementService.DeleteDependency(dependencyId);
            return Ok();
        }
    }  
    
}