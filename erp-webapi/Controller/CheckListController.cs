﻿using System.Collections.Generic;
using System.Threading.Tasks;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Services;
using flexli_erp_webapi.DataModels;
using Microsoft.AspNetCore.Mvc;
using flexli_erp_webapi.Models;
using Microsoft.AspNetCore.Cors;


namespace flexli_erp_webapi.Controller
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [EnableCors("AllowOrigin")]
    [Produces("application/json")]
    
    public class CheckListController : ControllerBase
    {
        /// <summary>
        /// [R]Get Check List for a Task or Template.checkListType = ["Task,"Template"]
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetCheckList")]
        [Consumes("application/json")]
        
        public List<CheckListItemEditModel> GetCheckListByTypeId(string typeId, ECheckListType checkListType)
        {
            return CheckListManagementService.GetCheckList(typeId, checkListType);
        }
        /// <summary>
        /// [R]Checklist can added only if sprint is in planning stage
        /// Checklist params could be modified based upon sprint state. Eg, description cannot be cannot be change once froze
        /// Not used for Updating Manager's comment and Approving checklist. (Check Sprint Report)
        /// result-Type {"File","Numeric","Boolean"} Status {"NotCompleted","Completed"}
        /// </summary>
        /// <returns></returns>
        [HttpPut("CreateOrUpdateCheckListItem")]
        [Consumes("application/json")]
        
        public CheckListItemEditModel CreateOrUpdateCheckListItem(CheckListItemEditModel checkListItemItem)
        {
            return CheckListManagementService.CreateOrUpdateCheckListItem(checkListItemItem);
        }
        /// <summary>
        /// [R]Checklist can be deleted only if sprint is in planning stage/not linked to sprint
        /// </summary>
        /// <returns></returns>
        
        [HttpDelete("DeleteCheckListItem")]
        [Consumes("application/json")]
        
        public ActionResult<string> DeleteCheckListItem(string checkListItemId)
        {
            CheckListManagementService.DeleteCheckListItem(checkListItemId);
            return Ok();
        }
    }
}