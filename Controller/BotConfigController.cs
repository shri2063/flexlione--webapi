using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using m_sort_server.Interfaces;
using m_sort_server.Models;
using m_sort_server.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using BotConfig = m_sort_server.Models.BotConfig;

namespace m_sort_server.Controller
{
    [Route("api/v1/[controller]")]
    [ApiController]
    
    [ApiVersion("1.0")]
    [EnableCors("AllowOrigin")]
    [Produces("application/json")]
    public class BotConfigController : ControllerBase
    {
        private readonly IBotManager _botManager;
       
        public BotConfigController (IBotManager botManager)
        {
            _botManager = botManager;
        }

        [HttpGet("GetBots")]
        [Consumes("application/json")]
        public ActionResult<List<Bot>> GetBots(string botId)
        {
            return _botManager.GetBots(botId);
        }

        [HttpPut("AddBot")]
        [Consumes("application/json")]
        public ActionResult<string> AddBot(string ipAddress)
        {
            _botManager.AddBot(ipAddress);
            return Ok();
        } 
        
        [HttpPut("DeleteBot")]
        [Consumes("application/json")]
        public ActionResult<string> DeleteBot(string botId)
        {
            _botManager.RemoveBot(botId);
            return Ok();
        }

        [HttpPost("SetPower")]
        [Consumes("application/json")]
        public ActionResult<string> SetPower(string botId, bool power)
        {
            _botManager.SetPower(botId,power);
            return Ok();
        }

       
    }
}