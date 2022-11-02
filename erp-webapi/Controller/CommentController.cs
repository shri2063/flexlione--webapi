using System.Collections.Generic;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Services;
using flexli_erp_webapi.BsonModels;
using flexli_erp_webapi.DataModels;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace flexli_erp_webapi.Controller
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [EnableCors("AllowOrigin")]
    [Produces("application/json")]
    
    public class CommentController:ControllerBase
    {
        
        [HttpGet("GetCommentsByTaskId")]
        [Consumes("application/json")]
        
        public List<CommentEditModel> GetCommentsByTaskId(string taskId, int? pageIndex = null, int? pageSize = null)
        {
            return CommentManagementService.GetCommentsByTaskId(taskId, pageIndex, pageSize);
        }
        
        
        /// <summary>
        /// Create or update tasks
        /// Note that if you are creating new taskDetail - what taskDetail Id you mention does not matter
        /// It will assign serially
        /// position after you can keep empty ("")
        /// </summary>
        [HttpPut("CreateOrUpdateComment")]
        [Consumes("application/json")]
        
        public ActionResult<CommentEditModel> CreateOrUpdateComment(CommentEditModel comment)
        {
            return CommentManagementService.CreateOrUpdateComment(comment);
        }


        [HttpDelete("DeleteComment")]
        [Consumes("application/json")]
        
        public ActionResult<string> DeleteComment(string commentId)
        {
            CommentManagementService.DeleteComment(commentId);
            return Ok();
        }
    }
}