using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using m_sort_server.BsonModels;
using m_sort_server.DataModels;
using m_sort_server.EditModels;
using m_sort_server.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace m_sort_server.Controller
{
    
    [Route("api/v1/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [EnableCors("AllowOrigin")]
    [Produces("application/json")]

    public class SearchController : ControllerBase
    {

        [HttpPost("AddOrUpdateTagWithResult")]
        [Consumes("application/json")]

        public SearchTagResult AddOrUpdateTagWithResult(string tag)
        {
            return TagManagementService.AddOrUpdateTagWithResult(tag);
        }
        
        
        
        [HttpGet("GetTaskListForTag")]
        [Consumes("application/json")]

        public SearchTagResult GetTaskListForTag(string search)
        {
            return TagManagementService.GetSearchTagResult("description", search);
        }
        
        
        
        
        
        [HttpPost("GetSearchResult")]
        [Consumes("application/json")]

        // Note: Here we are using TaskDetailModel Object for both Tag vs Detail Search
        // In case of Tag Search, we will will convert Search model into TaskDetail model
        public ActionResult<List<TaskDetailEditModel>> GetSearchResult(SearchQueryEditModel searchQuery)
        {
            return SearchManagementService.GetTaskListForSearchQuery(searchQuery);
        }
        
        [HttpGet("GetTagList")]
        [Consumes("application/json")]

        public ActionResult<List<SearchTag>> GetTagList()
        {
            return TagManagementService.GetSearchTagList();
        }
        
    }

}