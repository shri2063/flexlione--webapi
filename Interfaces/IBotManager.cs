using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using m_sort_server.Models;

namespace m_sort_server.Interfaces
{
    public interface IBotManager
    {
        List<Bot> GetBots(string botId = null);

        void AddBot(string ipAddress);

        List<Bot> Initialize();
        
        void RemoveBot(string botId);

        void SetMotor(string botId, Motor motor);
        
        void SetPower(string botId, bool power);

        
    }
}