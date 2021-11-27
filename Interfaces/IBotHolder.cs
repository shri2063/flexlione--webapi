using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using m_sort_server.Models;

namespace m_sort_server.Interfaces
{
    public interface IBotHolder
    {
        void AddBot(BotConfig botConfig);

        void RemoveBot(string botId);

        void SetCommand(string botId,string value);

        string GetCommand(string botId);
        
        void SetActualSpeed(string botId,string value);

        string GetActualSpeed(string botId);
        BotPosition GetPosition(string botId);

        BotConfig GetConfig(string botId);
        
        BotConfig GetPowerStatus(string botId);
        
        BotMotor GetMotorStatus(string botId);
        
        List<string> GetBotIds( );
        
        List<string> GetPoweredBotIds( );
        
        List<string> GetActiveMotorBotIds( );
        
        void SetPosition(string botId, BotPosition botPosition);
        
        void SetConfig(string botId, BotConfig botConfig);
        
        void SetMotorStatus(string botId, BotMotor botMotor);

        void SetPowerStatus(string botId, bool power);

        Node GetNode(string botId);
        
        Station GetStation(string botId);

        void SetSpeed(string botId, decimal speed);

        decimal GetSpeed(string botId);

        void UpdateNode(string botId, string nodeId, DateTime time);
        
        void UpdateStation(string botId, string stationId, DateTime time);
    }
}