using System.Collections.Generic;
using System.Runtime.InteropServices;
using m_sort_server.Models;

namespace m_sort_server.Interfaces
{
    public interface IBotConfig
    {
        void AddBotConfig(string botId);

        void RemoveBotConfig(string botId);
        void SetBotPower(string botId, bool power);

        List<BotConfig> InitializeBotConfigsFromLogs();

        void ClearSetup();


        List<string> GetBotIds();
        dynamic GetBotConfig([Optional] string botId);

    }
}