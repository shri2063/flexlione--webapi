using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using m_sort_server.Interfaces;
using m_sort_server.Models;

namespace m_sort_server.Services
{
    public class BotConfig:IBotConfig
    {
       
        

        private readonly ILogFileService _logFileService;
        
        private readonly IBotHolder _botHolder;

        public BotConfig(ILogFileService logFileService, IBotHolder botHolder)
        {
            _logFileService = logFileService;
            _botHolder = botHolder;
        }


        public List<Models.BotConfig> InitializeBotConfigsFromLogs()
        {
            List<Models.BotConfig> botConfigs = _logFileService
                .ReadFromJsonFile<Models.BotConfig>("bot_config");
            try
            {
                botConfigs
                    .ForEach(x => _botHolder
                        .AddBot(x));
                return botConfigs;
            }
            catch
            {
                return null;
            }
            
           

        }

        public void ClearSetup()
        {
            List<string> botIds = _botHolder.GetBotIds();
            botIds.ForEach(x =>_botHolder.RemoveBot(x));
        }

        public List<string> GetBotIds()
        {
            return _botHolder.GetBotIds();
        }
        
        public dynamic GetBotConfig(string botId = null)
        {
            if (botId == null)
            {
                List<Models.BotConfig> botConfigs = new List<Models.BotConfig>();
                _botHolder.GetBotIds()
                    .ForEach(x => botConfigs
                        .Add(_botHolder.GetConfig(x)));
                
                return botConfigs;
            }

            return _botHolder.GetConfig(botId);
        }

        public void AddBotConfig(string ipAddress)
        {

            List<Models.BotConfig> botConfigs = _logFileService.ReadFromJsonFile<Models.BotConfig>("bot_config");
           
            Models.BotConfig addBotConfig = new Models.BotConfig();
            // Check if IP already allocated
            try
            {
                if (botConfigs.Find(x =>
                    x.IpAddress == ipAddress) != null)
                {
                    return;
                }

                int nextBotId = botConfigs.Count + 1;
                 addBotConfig = new Models.BotConfig()
                {
                    BotId = nextBotId.ToString(),
                    Power = false,
                    IpAddress = ipAddress
                };
            }
            catch(NullReferenceException)
            {
                addBotConfig = new Models.BotConfig()
                {
                    BotId = "1",
                    Power = false,
                    IpAddress = ipAddress
                };
            }

            _logFileService.WriteIntoJsonFile<Models.BotConfig>(addBotConfig,"bot_config");

            _botHolder.AddBot(addBotConfig);


        }

        public void RemoveBotConfig(string botId)
        {
            if(_botHolder.GetConfig(botId) == null)
            {
                throw new Exception("Bot Id does not exist");
            }

            List<Models.BotConfig> botConfigs = _logFileService.ReadFromJsonFile<Models.BotConfig>("bot_config");

            botConfigs.RemoveAll(x => x.BotId == botId);
            OverWriteBotConfigLogs(botConfigs);

            _botHolder.RemoveBot(botId);
        }
        
        

        public void  SetBotPower(string botId, bool power)
        {
           
            if(_botHolder.GetConfig(botId) == null)
            {
                throw new Exception("Bot Id does not exist");
            }
            _botHolder.SetPowerStatus(botId,power);
            UpdateBotConfigLogs(_botHolder.GetConfig(botId));
        }

       


        private void OverWriteBotConfigLogs(List<Models.BotConfig> bots)
        {
            _logFileService.DeleteJsonFile("bot_config");
            bots.ForEach(x =>
                _logFileService.WriteIntoJsonFile<Models.BotConfig>(x,"bot_config") );
            
        }
        
        private void UpdateBotConfigLogs(Models.BotConfig bot)
        {
            if(_botHolder.GetConfig(bot.BotId) == null)
            {
                throw new Exception("Bot Id does not exist");
            }
            List<Models.BotConfig> botConfigs = new List<Models.BotConfig>();
            _botHolder.GetBotIds()
                .ForEach(x => botConfigs
                    .Add(_botHolder.GetConfig(x)));
            OverWriteBotConfigLogs(botConfigs);

        }
    }
}