using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using m_sort_server.Interfaces;
using m_sort_server.Models;

namespace m_sort_server.Services
{
    public class BotOperator:IBotOperator
    {
        private  readonly  ILogFileService  _logFileService;
        private  readonly  IBotHolder  _botHolder;

        public BotOperator(ILogFileService logFileService, IBotHolder botHolder)
        {
            _logFileService = logFileService;
            _botHolder = botHolder;
        }


        

        public dynamic GetBotMotorStatus(string botId = null)
        {
            if (botId == null)
            {
                List<BotMotor> botMotorStatusList = new List<BotMotor>();
                _botHolder.GetPoweredBotIds()
                    .ForEach(x => botMotorStatusList
                        .Add(_botHolder.GetMotorStatus(x)));
                
                return botMotorStatusList;
            }

            return _botHolder.GetMotorStatus(botId);
        }

        public void SetMotor(Motor motor,string botId = null)
        {
           
            if (botId == null)
            {
                List<BotMotor> botMotorStatusList = new List<BotMotor>();
                _botHolder.GetPoweredBotIds()
                    .ForEach(x =>
                        botMotorStatusList.Add(_botHolder.GetMotorStatus(x)));
                botMotorStatusList
                    .ForEach(x =>
                    {
                        x.Motor = motor;
                        _botHolder.SetCommand(x.BotId,Convert.ToInt32(
                            motor).ToString());
                        UpdateMotorStateLog(x);
                        _botHolder.SetMotorStatus(x.BotId,x);
                    });
            }
            else
            {
                if (_botHolder.GetPowerStatus(botId).Power== false)
                {
                    throw new Exception("Bot is not powered");
                }
                BotMotor botMotor = _botHolder
                    .GetMotorStatus(botId);
                botMotor.Motor = motor;
                _botHolder.SetCommand(botId,Convert.ToInt32(
                    motor).ToString());
                UpdateMotorStateLog(botMotor);
                _botHolder.SetMotorStatus(botId,botMotor);
            }
        }

        public void SetCommand(string botId, string command)
        {
            _botHolder.SetCommand(botId,command);
        }


        private void UpdateMotorStateLog(BotMotor botMotor)
        {
           WriteJson(botMotor);
            
        }

        private void WriteJson(BotMotor botMotor)
        {
            string filename = "bot_motor";
            try
            {
                _logFileService.WriteIntoJsonFile<BotMotor>(botMotor,filename);
            }
            catch (System.IO.IOException)
            {
                Thread.Sleep(100);
                WriteJson(botMotor);
            } 
        }
    }
}