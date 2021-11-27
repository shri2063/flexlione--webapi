using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using m_sort_server.Interfaces;
using m_sort_server.Models;

namespace m_sort_server.Services
{
    public class BotHolder : IBotHolder
    {
        private readonly List<Bot> _bots = new List<Bot>();

        public void AddBot(Models.BotConfig botConfig)
        {
            _bots.Add(new Bot()
            {
                BotId = botConfig.BotId,
                BotConfig = botConfig,
                BotMotor = new BotMotor()
                {
                    BotId = botConfig.BotId,
                    Motor = Motor.Min
                },
                BotPosition = new BotPosition()
                {
                    BotId = botConfig.BotId,
                    NodeId = "0",
                    StationId = "0",
                    Time = DateTime.Now
                },
                Node = new Node()
                {
                    NodeId = "0",
                    Time = DateTime.Now,
                    LastNode = new Node()
                },
                Station = new Station()
                {
                    StationId = "0",
                    Time = DateTime.Now,
                    LastStation = new Station()
                }
                
            });
        }

       
        
        public void RemoveBot(string botId)
        {
            _bots.RemoveAll(x => x.BotId == botId);
        }

        
        public void SetCommand(string botId,string value)
        {
            GetBot(botId).BotPosition.Command = value;
        }

        public string GetCommand(string botId)
        {
            return GetBot(botId).BotPosition.Command;
        }

        public void SetActualSpeed(string botId, string value)
        {
            GetBot(botId).BotPosition.ActualSpeed = value;
        }

        public string GetActualSpeed(string botId)
        {
            return GetBot(botId).BotPosition.ActualSpeed;
        }

        public BotPosition GetPosition(string botId)
        {
            Bot bot = _bots.Find(x => x.BotId == botId);
            try
            {
                return bot.BotPosition;
            }
            catch 
            {
                return null;
            }
        }

       

        public Models.BotConfig GetConfig(string botId)
        {
            return _bots.Find(x => x.BotId == botId)
                .BotConfig;
        }

        Models.BotConfig IBotHolder.GetPowerStatus(string botId)
        {
            return _bots.Find(x => x.BotId == botId)
                .BotConfig;
        }

        public BotMotor GetMotorStatus(string botId)
        {
            return _bots.Find(x => x.BotId == botId)
                .BotMotor;
        }

        

        public List<string> GetBotIds()
        {
            List<string> botIds = new List<string>();

            _bots.ForEach(x => botIds.Add(x.BotId));
            return botIds;
        }

        public List<string> GetPoweredBotIds()
        {
            List<string> botIds = new List<string>();

            _bots.ForEach(x =>
            {
                if (x.BotConfig.Power)
                {
                    botIds.Add(x.BotId);
                }
            });
            return botIds;
        }

        public List<string> GetActiveMotorBotIds()
        {
            List<string> botIds = new List<string>();

            _bots.ForEach(x =>
            {
                if (x.BotMotor.Motor != Motor.Min)
                {
                    botIds.Add(x.BotId);
                }
            });
            return botIds;
        }

        public void SetPosition(string botId, BotPosition botPosition)
        {

            BotPosition currentPosition =
                GetBot(botId)
                    .BotPosition;
            currentPosition.NodeId = botPosition.NodeId;
            currentPosition.StationId = botPosition.StationId;
            currentPosition.Time = botPosition.Time;

        }

        public void SetConfig(string botId, Models.BotConfig botConfig)
        {
            GetBot(botId)
                .BotConfig = botConfig;
        }

        public void SetMotorStatus(string botId, BotMotor botMotor)
        {
            GetBot(botId)
                .BotMotor = botMotor;
        }

        
        
        public void SetPowerStatus(string botId, bool power)
        {
            GetBot(botId)
                .BotConfig.Power = power;
        }

        public Node GetNode(string botId)
        {
            return GetBot(botId)
                .Node;
        }

        public Station GetStation(string botId)
        {
            return GetBot(botId)
                .Station;
        }

        public decimal GetSpeed(string botId)
        {
            return GetBot(botId).BotPosition.CalAvgSpeed;
        }
        
        public void SetSpeed(string botId, decimal speed)
        {
            GetBot(botId)
                .BotPosition.CalAvgSpeed = Decimal.Round(speed, 2);
        }

        public void UpdateNode(string botId, string nodeId, DateTime time)
        {
            Bot bot = GetBot(botId);
            bot.Node.LastNode = new Node()
            {
                NodeId = bot.Node.NodeId,
                Time = bot.Node.Time,
                Pointer = bot.Node.Pointer
            };
            bot.Node.NodeId = nodeId;
            bot.Node.Time = time;
            bot.Node.Pointer = Convert.ToDecimal(
                bot.BotPosition.StationId + "." + nodeId);
        }
        
        public void UpdateStation(string botId, string stationId, DateTime time)
        {
            Bot bot = GetBot(botId);
            bot.Station.LastStation = new Station()
            {
                StationId = bot.Station.StationId,
                Time = bot.Station.Time,
            };
            bot.Station.StationId = stationId;
            bot.Station.Time = time;
            
        }

        private Bot GetBot(string botId)
        {
            return _bots.FindAll(x => x.BotId == botId)
                .First();
        }
    }
}