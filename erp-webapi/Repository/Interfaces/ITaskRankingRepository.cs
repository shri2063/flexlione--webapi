using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using flexli_erp_webapi.BsonModels;

namespace flexli_erp_webapi.Repository.Interfaces
{
    public interface ITaskRankingRepository
    {
        Task<List<String>> GetChildTaskRanking(string taskId);

        Task<List<String>> ReviseChildTaskRanking(string taskId,  List<String> revisedList);

        
    }
}