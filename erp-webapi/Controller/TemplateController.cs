using System;
using System.Collections.Generic;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Services;
using flexli_erp_webapi.DataModels;
using flexli_erp_webapi.Services.Interfaces;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace flexli_erp_webapi.Controller
{

    [Route("api/v1/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [EnableCors("AllowOrigin")]
    [Produces("application/json")]

    
    public class TemplateController : ControllerBase
    {

        private readonly ITemplateManagementService _templateManagementService;
        private readonly TemplateMainService _templateMainService;

        ///<Summary>
        /// ToDo
        ///</Summary>
        public TemplateController(ITemplateManagementService templateManagementService, TemplateMainService templateMainService)
        {
            _templateManagementService = templateManagementService;
            _templateMainService = templateMainService;
        }
        /// <summary>
        /// Include - ["children","parent"]
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetTemplateById")]
        [Consumes("application/json")]

       
        public TemplateEditModel GetTemplateById(string templateId, List<string> include = null)
        {
            return _templateMainService.GetTemplateById(templateId, include);
        }
        
        ///<Summary>
        ///</Summary>
        [HttpGet("GetSimilarTemplateList")]
        [Consumes("application/json")]

       
        public List<TemplateEditModel> GetSimilarTemplateList(string templateId = null, int? pageIndex = null, int? pageSize = null)
        {
            return _templateManagementService.GetSimilarTemplateList(templateId, pageIndex, pageSize);
        }
        
        ///<Summary>
        /// ToDo
        ///</Summary>
        [HttpPost("ReplaceChildTemplate")]
        [Consumes("application/json")]

        public TemplateEditModel ReplaceChildTemplate(string oldTemplateId, string newTemplateId, string parentTemplateId)
        {
            return _templateManagementService.ReplaceChildTemplate(oldTemplateId, newTemplateId, parentTemplateId);
        }
        
        ///<summary>
        /// 
        ///</summary>
        [HttpPost("CreateOrUpdateTemplate")]
        [Consumes("application/json")]

        public TemplateEditModel CreateOrUpdateTemplate(TemplateEditModel template)
        {
            return _templateManagementService.CreateOrUpdateTemplate(template);
            
        }
        
        ///<Summary>
        /// ToDo
        ///</Summary>
        [HttpPut("CloneTemplate")]
        [Consumes("application/json")]

        public TemplateEditModel CloneTemplate(string templatedId)
        {
            return _templateManagementService.CloneTemplate(templatedId);
        }
        
       
        
        ///<summary>
        /// [Check] : Parent Template exists
        /// [Check] : Child Template exists
        ///</summary>
        [HttpPost("AddChildTemplate")]
        [Consumes("application/json")]

        public TemplateEditModel AddChildTemplate(string childTemplateId, string parentTemplateId)
        {
            return _templateManagementService.AddChildTemplate(childTemplateId, parentTemplateId);
            
        }
        
        ///<summary>
        /// [Check] : Parent Template exists
        /// [NoCheck]:  template exists in child template list, child template exists
        ///</summary>
        [HttpPost("RemoveChildTemplate")]
        [Consumes("application/json")]

        public TemplateEditModel RemoveChildTemplate(string childTemplateId, string parentTemplateId)
        {
            return _templateManagementService.RemoveChildTemplate(childTemplateId, parentTemplateId);
            
        }
        
       
        ///<summary>
        /// 
        ///</summary>
        [HttpPost("GenerateTaskFromTemplate")]
        [Consumes("application/json")]

        public List<TaskDetailEditModel> GenerateTaskFromTemplate(TaskTemplateEditModel taskTemplate)
        {
            return _templateMainService.GenerateMultipleTaskFromTemplate(taskTemplate);

        }



        
        ///<summary>
        /// If include is null -> Only immediate reference template + immediate children roles will be included
        /// If include = "allChildren" -> then  immediate reference template + exhaustive children roles will be included
        ///</summary>
        [HttpPost("GetAllRolesForTemplateId")]
        [Consumes("application/json")]

        public List<string> GetAllRolesForTemplateId(string templateId, List<string> include = null)
        {
            return _templateMainService.GetAllRolesForTemplateId(templateId, include);

        }

        





















        /*
        [HttpGet("GetTemplateById")]
        [Consumes("application/json")]

        public TemplateEditModel GetTemplateById(string templateId, string include = null)
        {
            return TemplateManagementService.GetTemplateById(templateId, include);
        }
        
        [HttpGet("GetAllTemplates")]
        [Consumes("application/json")]

        public List<TemplateEditModel> GetAllTemplates()
        {
            return TemplateManagementService.GetAllTemplates();
        }
        
        [HttpPut("AddOrEditTemplate")]
        [Consumes("application/json")]

        public TemplateEditModel AddOrEditTemplate(TemplateEditModel template)
        {
            return TemplateManagementService.AddOrUpdateTemplate(template);
        }
        
        [HttpPost("AddTaskListToTemplate")]
        [Consumes("application/json")]

        public TemplateEditModel AddTaskListToTemplate(List<string> taskIdList, string templateId)
        {
            return TemplateManagementService.AddTaskListToTemplate(taskIdList,templateId);
        }
        
        [HttpPost("RemoveTaskListToTemplate")]
        [Consumes("application/json")]

        public ActionResult<string> RemoveTaskListFromTemplate(List<string> taskIdList, string templateId)
        {
            TemplateManagementService.RemoveTaskListFromTemplate(taskIdList,templateId);
            return Ok();
        }
        
        [HttpDelete("DeleteTemplate")]
        [Consumes("application/json")]

        public ActionResult<string> DeleteTemplateById(string templateId)
        {
            TemplateManagementService.DeleteTemplate(templateId);
            return Ok();
        }
    }
    */

    }
}