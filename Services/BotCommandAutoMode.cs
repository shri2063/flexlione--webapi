using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using m_sort_server.Interfaces;
using m_sort_server.Models;

namespace m_sort_server.Services
{
    public class BotCommandAutoMode : IBotCommand
    {

        private readonly IBotConfig _botConfig;
        private readonly IBotOperator _botOperator;
        private readonly IBotManager _botManager;
        private readonly ITransportOperator _transportOperator;
        private  readonly  ILogFileService  _logFileService;


        public BotCommandAutoMode(IBotConfig botConfig, IBotOperator botOperator, 
            IBotManager botManager, ITransportOperator transportOperator, ILogFileService logFileService)
        {
            _botConfig = botConfig;
            _botOperator = botOperator;
            _botManager = botManager;
            _transportOperator = transportOperator;
            _logFileService = logFileService;
        }

        public List<Bot> EstablishConnection()
        {
            List<string> botIds = _botConfig.GetBotIds();
            botIds.ForEach(x =>
                _botConfig.SetBotPower(x, true));
            return _botManager.GetBots();
        }

        

        public void Orchestrate()
        {
           
            while (true)
            {
                _transportOperator.ReadBotPositionFromLog();

                List<Bot> bots = _botManager.GetBots();
                

                bots
                    .ForEach(x =>
                    {

                        if (StartBot(x))
                        {
                            _botOperator.SetMotor(Motor.Max, x.BotId); 
                        }  
                        
                        if (StopBot(x))
                        {
                            _botOperator.SetMotor(Motor.Min, x.BotId); 
                        }
                        
                        UpdateBotWhereabouts(x);
                    });
            } 
        }
        
        public bool StopBot(Bot bot)
        {
            int currentStation = Convert.ToInt32(bot.BotPosition.StationId);

            if (currentStation > Convert.ToInt32(bot.Station.StationId))
            {
                if (currentStation %3 == 0 && Convert.ToInt32(bot.BotMotor.Motor) == 1)
                {
                    return true;
                }
            }

            return false;
        }

        public void UpdateBotWhereabouts(Bot bot)
        {
           
            int currentNode = Convert.ToInt32(bot.BotPosition.NodeId);
            
            // Update Bot feedback
            if (currentNode != Convert.ToInt32(bot.Node.NodeId))
            {
                _transportOperator.WriteBotFeedBackInLog(bot.BotId);
            }
            _transportOperator.Update(bot);
        }
        
        

        public bool StartBot(Bot bot)
        {
            if (Convert.ToInt32(bot.BotMotor.Motor) == 0 && bot.BotPosition.CalAvgSpeed < 0.5m)
            {
                return true;
            }

            return false;
        }
        
        
    }
}