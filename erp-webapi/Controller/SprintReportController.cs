using System.Collections.Generic;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Repository.Interfaces;
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
        
        private readonly ISprintReportRepository _sprintReportRepository;
        private readonly SprintReportManagementService _sprintReportManagementService;

        public SprintReportController(ISprintReportRepository sprintReportRepository,
            SprintReportManagementService sprintReportManagementService)
        {
            _sprintReportRepository = sprintReportRepository;
            _sprintReportManagementService = sprintReportManagementService;
            
        }

        /// <summary>
        /// [R]Use to fetch Sprint report for a sprint
        /// [Note]: Sprint No value is not  correct
        /// </summary>
        /// <returns></returns>
        
        [HttpGet("GetSprintReportById")]
        [Consumes("application/json")]
        
       
        public List<SprintReportEditModel> GetSprintReportForSprint(string sprintId, int? pageIndex = null, int? pageSize = null)
        {
            return _sprintReportRepository.GetSprintReportForSprint(sprintId, pageIndex, pageSize);
        }


        /// <summary>
        /// [R]Used For Updating Manager's comments and Approval
        /// Sprint status should not be closed
        /// validate approver id
        ///  sprintReportLineItemId, Manager_comment (editable),Approved(editable)
        /// Approved values ("NoAction", "True", "False")
        /// </summary>
        /// <returns></returns>
        
        [HttpPost("reviewCheckList")]
        [Consumes("application/json")]
        
        public ActionResult<CheckListItemEditModel> ReviewCheckList(SprintReportEditModel sprintReportEditModel, string approverId)
        {
           
            return Ok( _sprintReportManagementService.ReviewCheckList(sprintReportEditModel, approverId));
        }
    }
}