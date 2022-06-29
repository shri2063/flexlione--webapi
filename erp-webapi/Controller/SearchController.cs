using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using flexli_erp_webapi.BsonModels;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Services;
using flexli_erp_webapi.DataModels;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace flexli_erp_webapi.Controller
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