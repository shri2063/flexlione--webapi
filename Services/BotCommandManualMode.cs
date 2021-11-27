using System;
using System.Collections.Generic;
using m_sort_server.Interfaces;
using m_sort_server.Models;

namespace m_sort_server.Services
{
    public class BotCommandManualMode:IBotCommand
    {
        
        private readonly IBotConfig _botConfig;
        private readonly IBotOperator _botOperator;
        private readonly IBotManager _botManager;
        private readonly ITransportOperator _transportOperator;


        public BotCommandManualMode(IBotConfig botConfig, IBotOperator botOperator, 
            IBotManager botManager, ITransportOperator transportOperator)
        {
            _botConfig = botConfig;
            _botOperator = botOperator;
            _botManager = botManager;
            _transportOperator = transportOperator;
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
                        UpdateBotWhereabouts(x);
                    });
            }
        }
        
        public void UpdateBotWhereabouts(Bot bot)
        {
           
            int currentNode = Convert.ToInt32(bot.BotPosition.NodeId);
            _transportOperator.Update(bot);
            
            // Update Bot feedback
            if (currentNode != Convert.ToInt32(bot.Node.NodeId))
            {
                _transportOperator.WriteBotFeedBackInLog(bot.BotId);
            }
            
        }

        public bool StartBot(Bot bot)
        {
            throw new NotImplementedException();
        }

        public bool StopBot(Bot bot)
        {
            throw new NotImplementedException();
        }
    }
}