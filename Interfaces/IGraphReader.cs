using m_sort_server.Models;
using m_sort_server.Services;

namespace m_sort_server.Interfaces
{
    public interface IGraphReader
    {
         Graph GetGraph(ILogFileService logFileService);
    }
}