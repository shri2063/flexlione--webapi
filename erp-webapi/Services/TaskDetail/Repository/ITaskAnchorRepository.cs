using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using flexli_erp_webapi.BsonModels;

namespace flexli_erp_webapi.Repository.Interfaces
{
    public interface ITaskAnchorRepository
    {
        Task<List<String>> GetChildTaskRanking(string taskId);

        Task<List<String>> ReviseChildTaskRanking(string taskId,  List<String> revisedList);
        
        Task<List<String>> GetSearchResults(string taskId);

        Task<List<String>> ReviseSearchResults(string taskId,  List<String> revisedList);

        Task<List<String>> ReviseLabel(string taskId, List<String> revisedList);

        Task<List<String>> GetLabel(string taskId);


    }
}