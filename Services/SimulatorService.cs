using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using m_sort_server.Interfaces;
using m_sort_server.Models;

namespace m_sort_server.Services
{
    public class SimulatorService:ISimulatorService
    {

        private readonly ILogFileService _logFileService;
        private readonly IBotHolder _botHolder;
        private readonly List<SimulationBot> _simulationBots = new List<SimulationBot>();
        private decimal _simulationAcceleration;
        private decimal _simulationTimeStep;
        private decimal _simulationNodeDistance;
      
        
        
        
        public SimulatorService(ILogFileService logFileService,IBotHolder botHolder)
        {
            _logFileService = logFileService;
            _botHolder = botHolder;
           
        }


        private void Initialize(int bots, decimal timestep, decimal acceleration
        ,decimal nodeDistance)
        {
            int i = 1;
           
            while (i <= bots)
            {
                _simulationBots
                    .Add(new SimulationBot()
                    {
                        BotId = i.ToString(),
                        StationId = (i*100).ToString(),
                        NodeId = 0.ToString(),
                        PathTravelled = 0m,
                        Counter =  0,
                        Time = DateTime.Now
                        
                    });
                i++;
            }

            _simulationAcceleration = acceleration;
            _simulationTimeStep = timestep;
            _simulationNodeDistance = nodeDistance;
            
            //  Clear any past bot position file
            _logFileService.DeleteJsonFile("bot_position");
            
        }
        
        public void RunSimulation(int bots, decimal timeStep, 
            decimal acceleration, decimal nodeDistance)
        {
                     
            Initialize(bots, timeStep,acceleration,nodeDistance);
            while (true)
            {
                foreach (var bot in _simulationBots)
                {
                    UpdateSimulationBotSpeed(bot);
                // Simulation Speed would be set as actual Speed of bot
                    SetActualSpeedOfBot(bot);
                    bot.PathTravelled = bot.PathTravelled
                                        + bot.Speed * _simulationTimeStep;
                    
                    if (bot.PathTravelled >= 0.1m)
                    {
                        bot.Counter = bot.Counter + 1;
                        bot.PathTravelled = bot.PathTravelled - _simulationNodeDistance;
                    }
                    
                    bot.StationId = (Convert.ToInt32(bot.BotId)*100 + (bot.Counter/ 10)).ToString();
                    bot.NodeId = Convert.ToInt32(bot.Counter % (1/_simulationNodeDistance)).ToString();
                    bot.Time = bot.Time.AddSeconds(Convert.ToDouble(_simulationTimeStep));
                    UpdateBotPosition(bot);
                }

                Thread.Sleep(20);
            }
        }


        private void UpdateSimulationBotSpeed(SimulationBot bot)
        {
            decimal factor = Convert.ToInt32(GetMotorStatus(bot.BotId)) / 255.0m;
            if (bot.Speed < factor*1m)
            {
                bot.Speed = Math.Min(1m, bot.Speed + _simulationAcceleration * _simulationTimeStep);
         
            } else if (bot.Speed > factor*1m)
            {
                bot.Speed = Math.Max(0m, bot.Speed - _simulationAcceleration * _simulationTimeStep);
            }
            
            // Update Command Speed of Actual Bot
        }

        private void SetActualSpeedOfBot(SimulationBot bot)
        {
            _botHolder.SetActualSpeed(bot.BotId,Math.Round(bot.Speed,2).ToString());
        }

        private List<BotMotor> ReadJson()
        {
            try
            {
                return _logFileService.ReadFromJsonFile<BotMotor>("bot_motor");

            }
            catch 
            {
               Thread.Sleep(200);
               return ReadJson();
            }
        }
        
        private void WriteJson(BotPosition botPosition)
        {
            try
            {
                //_logFileService
                  //  .WriteIntoJsonFile<BotPosition>(botPosition,"bot_position");

            }
            catch 
            {
                WriteJson(botPosition);        
            }
        }
        
        private int GetMotorStatus(string botId)
        {
            List<BotMotor> botMotors;
            try
            {
                botMotors = ReadJson();
            }
            catch (IOException)
            {
                Thread.Sleep(200);
               botMotors = ReadJson();
            }
            
            return Convert.ToInt32(botMotors
                .FindLast(x => x.BotId == botId)
                .Motor);
        }


        
        private void UpdateBotPosition(SimulationBot bot)
        {
            BotPosition botPosition = new BotPosition()
            {
                BotId = bot.BotId,
                StationId = bot.StationId,
                NodeId = bot.NodeId,
                CalAvgSpeed = bot.Speed,
                Time = bot.Time
                
            };
            
            WriteJson(botPosition);
        }
        
        private class SimulationBot
        {
            public string BotId;
            public string StationId;
            public string NodeId;
            public decimal PathTravelled;
            public decimal Speed;
            public int Counter;
            public DateTime Time;
        }
    }
}