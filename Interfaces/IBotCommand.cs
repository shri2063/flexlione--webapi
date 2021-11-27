using System.Collections.Generic;
using m_sort_server.Models;

namespace m_sort_server.Interfaces
{
    public interface IBotCommand
    {
        List<Bot> EstablishConnection();

        void Orchestrate();

        bool StartBot(Bot bot);
        
        bool StopBot(Bot bot);

        void UpdateBotWhereabouts(Bot bot);
    }
}