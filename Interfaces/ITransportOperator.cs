using System.Collections.Generic;
using System.Runtime.InteropServices;
using m_sort_server.Models;

namespace m_sort_server.Interfaces
{
    public interface ITransportOperator
    {
        decimal GetBotSpeed(string botId);

        void ReadBotPositionFromLog( );

        void WriteBotFeedBackInLog(string botId);

        void Update(Bot bot);
        
       
        BotPosition GetBotPosition(string botId);

        Node GetNode(string botId);
        
        Station GetStation(string botId);
    }
}