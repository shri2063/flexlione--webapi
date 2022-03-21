using m_sort_server.BsonModels;
using m_sort_server.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace m_sort_server.Controller
{
    
    [Route("api/v1")]
    [ApiController]
    [ApiVersion("1.0")]
    [EnableCors("AllowOrigin")]
    [Produces("application/json")]

    public class TaskSearchController : ControllerBase
    {

        [HttpGet("GetCheckList")]
        [Consumes("application/json")]

        public SearchTag AddTag(string search)
        {
            return TagManagementService.AddTag(search);
        }
    }

}