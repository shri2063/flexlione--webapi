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

        ///<Summary>
        /// ToDo
        ///</Summary>
        public TemplateController(ITemplateManagementService templateManagementService)
        {
            _templateManagementService = templateManagementService;
        }
        /// <summary>
        /// Include - child
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetTemplateById")]
        [Consumes("application/json")]

       
        public TemplateEditModel GetTemplateById(string templateId, List<string> include = null)
        {
            return _templateManagementService.GetTemplateById(templateId, include);
        }
        
        ///<Summary>
        ///</Summary>
        [HttpGet("GetTemplateList")]
        [Consumes("application/json")]

       
        public List<TemplateEditModel> GetTemplateList()
        {
            return _templateManagementService.GetTemplateList();
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
        /// [ToDo]: Cannot add/remove child templates from here
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
        /// [Check] Cannot delete template if child template exist
        /// [No Check] No check for parent templates currently
        /// [Check] Cannot delete if serves as a clone template
        ///</summary>
        /// <returns></returns>
        [HttpDelete("DeleteTemplate")]
        [Consumes("application/json")]

        public ActionResult DeleteTemplate(string templatedId)
        {
            _templateManagementService.DeleteTemplate(templatedId);
            return Ok();
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