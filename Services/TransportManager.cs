using System.Collections.Generic;
using m_sort_server.Interfaces;

namespace m_sort_server.Services
{
    public class BotTransportManager:ITransportManager
    {
        private readonly ITransportOperator _transportOperator;
        private readonly IBotConfig _botConfig;

        BotTransportManager(ITransportOperator transportOperator,IBotConfig botConfig)
        {
            _transportOperator = transportOperator;
            _botConfig = botConfig;

        }


        public bool GetClearanceForBot(string botId)
        {
            throw new System.NotImplementedException();
        }
    }
    
    
}