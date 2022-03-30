using System.Collections.Generic;
using m_sort_server.BsonModels;
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
    
    public class CommentController:ControllerBase
    {
        
        [HttpGet("GetCommentsByTaskId")]
        [Consumes("application/json")]
        
        public List<CommentEditModel> GetCommentsByTaskId(string taskId)
        {
            return CommentManagementService.GetCommentsByTaskId(taskId);
        }
        
        
        /// <summary>
        /// Create or update tasks
        /// Note that if you are creating new task - what task Id you mention does not matter
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