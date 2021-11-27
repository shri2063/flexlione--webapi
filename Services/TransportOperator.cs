using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using m_sort_server.Interfaces;
using m_sort_server.Models;

namespace m_sort_server.Services
{
    public class TransportOperator: ITransportOperator
    {
        private  readonly  ILogFileService  _logFileService;
        private  readonly  IBotHolder  _botHolder;

        public TransportOperator(ILogFileService logFileService, IBotHolder botHolder)
        {
            _logFileService = logFileService;
            _botHolder = botHolder;
        }




        private List<BotPosition> ReadJson()
        {
            try
            {
                return _logFileService.ReadFromJsonFile<BotPosition>("bot_position");

            }
            catch 
            {
                Thread.Sleep(100);
                return ReadJson();
            }
        }


        public void ReadBotPositionFromLog()
        {
            List<BotPosition> botPositions = ReadJson();
            List<string> powerBotIds = _botHolder.GetPoweredBotIds();
            powerBotIds
                .ForEach(x =>
                {
                    BotPosition position = botPositions.FindLast(y =>
                        y.BotId == x);
                    if (position != null)
                    {
                        _botHolder.SetPosition(x, position);
                    }
                });
        }

        public void Update(Bot bot)
        {
            UpdateBotPosition(bot);
            UpdateBotSpeed(bot);
        }
        
        private void UpdateBotSpeed(Bot bot)
        {

            BotPosition position = _botHolder.GetPosition(bot.BotId);
            Node node = _botHolder.GetNode(bot.BotId);
            decimal secondsTakenForCrossingLastNode =  Convert.ToDecimal(node.Time.Subtract(node.LastNode.Time).TotalSeconds);
            
            // Check if bot has halted
                        
            decimal secondsPastSinceLastNode = Convert.ToDecimal(position.Time.Subtract(_botHolder.GetNode(bot.BotId)
                .LastNode.Time).TotalSeconds);
                if (secondsPastSinceLastNode > 1m)
                {
                    _botHolder.SetSpeed(bot.BotId,0m);
                    return;
                }
                _botHolder.SetSpeed(bot.BotId,CalculateBotSpeed(bot.BotId,secondsTakenForCrossingLastNode));
        }

        private void UpdateBotPosition(Bot bot)
        {
            BotPosition position = _botHolder.GetPosition(bot.BotId);
            if (_botHolder.GetNode(bot.BotId).NodeId != position.NodeId)
            {
                _botHolder.UpdateNode(bot.BotId, position.NodeId, position.Time);
               
            }
            if (_botHolder.GetStation(bot.BotId).StationId != position.StationId)
            {
                _botHolder.UpdateStation(bot.BotId, position.StationId, position.Time);
            }
        }




        private decimal CalculateBotSpeed(string botId, decimal secondsForCrossingLastNode)
        {

            try
            {
                int length = Convert.ToInt32(_botHolder.GetNode(botId).NodeId) -
                             Convert.ToInt32(_botHolder.GetNode(botId).LastNode.NodeId);

                // If length is less than zero, then we are assuming that next station has been crossed, so we have to add 10
                // Let's last pointer was 7.9 and and 11.1 then length should be 1 - 9 + 10 = 2
                if (length < 0)
                {
                    length = length + 10;
                }

                return Convert.ToDecimal(length) / secondsForCrossingLastNode;
            }

            catch (DivideByZeroException)
            {
                return 0m;
            }
        }

        public decimal GetBotSpeed(string botId)
        {
            return _botHolder.GetSpeed(botId);
        }

       

        

        public BotPosition GetBotPosition(string botId)
        {
            return _botHolder.GetPosition(botId);
        }

        public Node GetNode(string botId)
        {
            return _botHolder.GetNode(botId);
        }

        public Station GetStation(string botId)
        {
           return _botHolder.GetStation(botId);
        }

        public void WriteBotFeedBackInLog(string botId)
        {
            
                    _logFileService.WriteIntoJsonFile<BotPosition>(
                        _botHolder.GetPosition(botId),"bot_feedback");
               
        }
    }
}