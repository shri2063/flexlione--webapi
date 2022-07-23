﻿using System.Collections.Generic;
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
    
    public class SprintReportController : ControllerBase
    {
        [HttpGet("GetSprintReportById")]
        [Consumes("application/json")]
        
        public SprintReportEditModel GetSprintReportItemById(string sprintReportLineItemId)
        {
            return SprintReportManagementService.GetSprintReportItemById(sprintReportLineItemId);
        }
        
        [HttpPut("UpdateSprintReportLineItem")]
        [Consumes("application/json")]
        
        public SprintReportEditModel UpdateSprintReportLineItem(SprintReportEditModel sprintReportEditModel, string approverId)
        {
            return SprintReportManagementService.UpdateSprintReportLineItem(sprintReportEditModel, approverId);
        }
    }
}