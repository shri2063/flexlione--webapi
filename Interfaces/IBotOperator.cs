using System.Runtime.InteropServices;
using m_sort_server.Models;

namespace m_sort_server.Interfaces
{
    public interface IBotOperator
    {

        dynamic GetBotMotorStatus([Optional] string botId);

        void SetMotor(Motor motor, string botId = null);

        void SetCommand(string botId, string command);
    }
}