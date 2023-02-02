using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using flexli_erp_webapi.BsonModels;
using flexli_erp_webapi.DataLayer.Interface;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Repository.Interfaces;
using MongoDB.Driver;

namespace flexli_erp_webapi.Repository
{
    public class TaskAnchorRepository : ITaskAnchorRepository
    {
        private readonly ITagContext _tagContext;

        public TaskAnchorRepository(ITagContext tagContext)
        {
            _tagContext = tagContext ?? throw new ArgumentNullException(nameof(tagContext));
        }
        
        public async Task<List<String>> GetChildTaskRanking(string taskId)
        {
            var taskAnchor =  await _tagContext
                .TaskAnchor
                .Find(x => x.TaskId ==taskId)
                .FirstOrDefaultAsync();
            if (taskAnchor == null)
            {
                return new List<String>();
            }
            return taskAnchor.ChildTaskOrder;
        }
        
        public async Task<List<String>> GetSearchResults(string taskId)
        {
            var searchresults =  await _tagContext
                .TaskAnchor
                .Find(x => x.TaskId ==taskId)
                .FirstOrDefaultAsync();
            if (searchresults == null)
            {
                return new List<String>();
            }
            return searchresults.SearchResults;
        }

        

        

        public async Task<List<String>> ReviseChildTaskRanking(string taskId, List<String> revisedList)
        {
            var taskAnchorExists =  await CheckIfTaskAnchorExisits(taskId);

            if (!taskAnchorExists)
            {
                return await  CreateTaskAnchor( new TaskAnchor()
                {
                
                    TaskId = taskId,
                    // In Case of Search Tag: populate all tasks containing search word
                    ChildTaskOrder = revisedList
                });
            }
            
            var filter = Builders<TaskAnchor>.Filter.Eq(e => e.TaskId, taskId) ;

            var update = Builders<TaskAnchor>.Update
                .Set(x => x.ChildTaskOrder, revisedList);

            await _tagContext.TaskAnchor.FindOneAndUpdateAsync(filter, update);

            return await GetChildTaskRanking(taskId);
        }
        
        public async Task<List<String>> ReviseSearchResults(string taskId, List<string> revisedList)
        {
            var taskAnchorExists =  await CheckIfTaskAnchorExisits(taskId);

            if (!taskAnchorExists)
            {
                return await  CreateTaskAnchor( new TaskAnchor()
                {
                
                    TaskId = taskId,
                    // In Case of Search Tag: populate all tasks containing search word
                    SearchResults = revisedList
                });
            }
            
            var filter = Builders<TaskAnchor>.Filter.Eq(e => e.TaskId, taskId) ;

            var update = Builders<TaskAnchor>.Update
                .Set(x => x.SearchResults, revisedList);

            await _tagContext.TaskAnchor.FindOneAndUpdateAsync(filter, update);

            return await GetSearchResults(taskId);
        }
        
      
        
        private  async Task<List<String>> CreateTaskAnchor(TaskAnchor taskAnchor)
        {
            await _tagContext.TaskAnchor.InsertOneAsync(taskAnchor);
            return  await GetSearchResults(taskAnchor.TaskId);
        }
        
        private async Task<bool> CheckIfTaskAnchorExisits(string taskId)
        {
            var taskAnchor =  _tagContext
                .TaskAnchor
                .Find(x => x.TaskId == taskId)
                .FirstOrDefault()
                ;
               
            if (taskAnchor == null)
            {
                return false;
            }

            return true;
        }
        
       
    }
}