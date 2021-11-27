using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using m_sort_server.Interfaces;
using m_sort_server.Models;
using Microsoft.AspNetCore.Mvc;

namespace m_sort_server.Controller
{
    public class BotCommandController :  ControllerBase
    {
        private readonly IBotManager _botManager;
        private readonly IBotCommand _botCommand;
        private readonly ISimulatorService _simulatorService;
       
        public BotCommandController (IBotManager botManager, IBotCommand botCommand,
            ISimulatorService simulatorService)
        {
            _botManager = botManager;
            _botCommand = botCommand;
            _simulatorService = simulatorService;
        }

        /*
        [HttpPost("Initialize")]
        [Consumes("application/json")]
        public ActionResult<List<Bot>> Initialize( )
        {
           return _botManager.Initialize();
        }
        
        [HttpPost("CheckConnection")]
        [Consumes("application/json")]
        public ActionResult<List<Bot>> EstablishConnection( )
        {
            if (_botManager.GetBots().Count == 0)
            {
                throw new Exception("Please initialize setup");
            }
            return _botCommand.EstablishConnection();
        }
        */
        
        [HttpPost("SetMotor")]
        [Consumes("application/json")]
        public ActionResult<string> SetMotor(string botId, Motor motor)
        {
        
            _botManager.Initialize();
            _botCommand.EstablishConnection();
            _botManager.SetMotor(botId,motor);
            return Ok();
        }
        
        [HttpPost("ManageTraffic")]
        [Consumes("application/json")]
        public void Orchestrate( )
        {
            //var t = Task.Run(() =>  _botCommand.Orchestrate());
            _botCommand.Orchestrate();

        }
        
        [HttpPost("RunSimulator")]
        [Consumes("application/json")]
        public void RunSimulator( int bots, decimal timeStep, 
            decimal acceleration, decimal nodeDistance)
        {
            //var t = Task.Run(() =>  _simulatorService.RunSimulation(bots,timeStep,acceleration,nodeDistance));
            _simulatorService.RunSimulation(bots, timeStep, acceleration, nodeDistance);
        }
        
        
        
        [HttpPost("SetVoltage")]
        [Consumes("application/json")]
        public void SetVoltage( string botId, Motor motor)
        {
             _botManager.SetMotor(botId,motor);
        }


        
        
        

        

    }
}