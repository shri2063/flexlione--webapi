using System;
using System.Collections.Generic;
using m_sort_server.DataModels;
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
    
    public class TemplateController : ControllerBase
    {
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
    
}