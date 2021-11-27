using System.Collections.Generic;
using m_sort_server.Interfaces;
using m_sort_server.Models;
using m_sort_server.Services;
using Microsoft.AspNetCore.Mvc;

namespace m_sort_server.Controller
{
    public class GraphCreationController : ControllerBase
    {

        private readonly ILogFileService _logFileService;
        
        public GraphCreationController (ILogFileService logFileService)
        {
            _logFileService = logFileService;
        }
        
        [HttpPost("GetEdges")]
        [Consumes("application/json")]
        
      
        
        public ActionResult<List<EdgeJsonModel>> GetEdges()
        {
        
            // Create a GraphDataModel
            Graph MGraph = new JsonGraphReader().GetGraph(_logFileService);
            return MGraph.EdgeJsonModels;
        }
    }
}