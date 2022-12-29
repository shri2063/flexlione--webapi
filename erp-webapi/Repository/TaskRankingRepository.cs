using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using flexli_erp_webapi.BsonModels;
using flexli_erp_webapi.DataLayer.Interface;
using flexli_erp_webapi.Repository.Interfaces;
using MongoDB.Driver;

namespace flexli_erp_webapi.Repository
{
    public class TaskRankingRepository : ITaskRankingRepository
    {
        private readonly ITagContext _tagContext;

        public TaskRankingRepository(ITagContext tagContext)
        {
            _tagContext = tagContext ?? throw new ArgumentNullException(nameof(tagContext));
        }
        
        public async Task<List<String>> GetChildTaskRanking(string taskId)
        {
            var taskHierarchy =  await _tagContext
                .TaskHierarchy
                .Find(x => x.TaskId ==taskId)
                .FirstOrDefaultAsync();
            if (taskHierarchy == null)
            {
                return new List<String>();
            }
            return taskHierarchy.ChildTaskOrder;
        }

        public async Task<List<String>> ReviseChildTaskRanking(string taskId, List<String> revisedList)
        {
            var filter = Builders<TaskHierarchy>.Filter.Eq(e => e.TaskId, taskId) ;

            var update = Builders<TaskHierarchy>.Update
                .Set(x => x.ChildTaskOrder, revisedList);

            await _tagContext.TaskHierarchy.FindOneAndUpdateAsync(filter, update);

            return await GetChildTaskRanking(taskId);
        }
    }
}