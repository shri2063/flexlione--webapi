using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using m_sort_server.Controller;
using m_sort_server.Interfaces;
using m_sort_server.Models;

namespace m_sort_server.Services
{
    public class BotManagers : IBotManager
    {
        private readonly IBotOperator _botOperator;
        private readonly IBotConfig _botConfig;
        private readonly ITransportOperator _transportOperator;

        public BotManagers(IBotOperator botOperator, IBotConfig botConfig, ITransportOperator transportOperator)
        {
            _botOperator = botOperator;
            _botConfig = botConfig;
            _transportOperator = transportOperator;
        }


        public List<Bot> Initialize()
        {    _botConfig.ClearSetup();
            _botConfig.InitializeBotConfigsFromLogs();
            return GetBots();

        }
        
        public List<Bot> GetBots(string botId = null)
        {
            List<string> botIds = _botConfig.GetBotIds();
            List<Bot> bots = new List<Bot>(); 
            botIds.ForEach(x =>
            {
                bots.Add(new Bot()
                {
                    BotId =x,
                    BotConfig = _botConfig.GetBotConfig(x),
                    BotMotor = _botOperator.GetBotMotorStatus(x),
                    BotPosition = _transportOperator.GetBotPosition(x),
                    Node = _transportOperator.GetNode(x),
                    Station = _transportOperator.GetStation(x)
                });
            });

            return bots;
        }

        public void AddBot(string ipAddress)
        {
            _botConfig.AddBotConfig(ipAddress);
        }

        public void RemoveBot(string botId)
        {
            _botConfig.RemoveBotConfig(botId);
        }

        public void SetMotor(string botId, Motor motor)
        {
            _botOperator.SetMotor(motor,botId);
        }
        public void SetPower(string botId, bool power)
        {
          _botConfig.SetBotPower(botId,power);
        }

       
    }
}