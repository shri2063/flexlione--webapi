using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using flexli_erp_webapi.BsonModels;
using flexli_erp_webapi.DataLayer.Interface;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Repository.Interfaces;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace flexli_erp_webapi.Services.TaskSearch
{
    public class TaskSearchResultRepository : ITaskSearchResultRepository
    {
        private readonly ITagContext _tagContext;
     

        public TaskSearchResultRepository(ITagContext tagContext, ITaskRepository taskRepository)
        {
            _tagContext = tagContext ?? throw new ArgumentNullException(nameof(tagContext));
           
           

        }

        public async Task<List<TaskSearchView>> SearchTasks(string keyword)
        {
           var tasktag =  await _tagContext
                .TaskSearchResult
                .Find(x => x.Keyword == keyword.ToLower())
                .FirstOrDefaultAsync();
           if (tasktag == null)
           {
               return new List<TaskSearchView>();
           }
           return tasktag.Tasks.ToList();
        }

      

        public async Task<List<TaskSearchView>> AddOrUpdateTask(string keyword, TaskSearchView task)
        {
            BsonModels.TaskSearch taskSearch = await GetTaskSearch(keyword.ToLower());
            
            // [Check]: Tag has already been added in TagTask DB
            if (taskSearch == null )
            {
                await CreateSearch(keyword);
              
            }

            var filter = Builders<BsonModels.TaskSearch>.Filter.Eq(e => e.Keyword, keyword.ToLower()) ; 
            var taskList = await SearchTasks(keyword.ToLower());
            taskList.RemoveAll(x => x.TaskId == task.TaskId);
            taskList.Add(task);

            var update = Builders<BsonModels.TaskSearch>.Update
                    .Set( x => x.Tasks, taskList);

            await _tagContext.TaskSearchResult.FindOneAndUpdateAsync(filter, update);
            

            return await SearchTasks(keyword);
        }

       

        public async  Task<BsonModels.TaskSearch> CreateSearch(string keyword)
        {
            BsonModels.TaskSearch newTaskSearch = new BsonModels.TaskSearch()
            {
                
                Keyword = keyword,
                // In Case of Search Tag: populate all tasks containing search word
                Tasks = new List<TaskSearchView>()
            };
            
            // [dbCheck]: Unique constraint on keyword + tagType
            await _tagContext.TaskSearchResult.InsertOneAsync(newTaskSearch);

            return await GetTaskSearch(keyword);
        }

       
       

       
        
        public async Task<List<TaskSearchView>> RemoveTask(string keyword, string taskId)
        {
            var filter = Builders<BsonModels.TaskSearch>.Filter.Eq(e => e.Keyword, keyword.ToLower()) ;

            var update = Builders<BsonModels.TaskSearch>.Update
                .PullFilter(e => e.Tasks, Builders<TaskSearchView>.Filter.Where(task => task.TaskId ==  taskId));

            await _tagContext.TaskSearchResult.FindOneAndUpdateAsync(filter, update);

            return await SearchTasks(keyword);
        }

        public async Task<List<string>> GetListOfTaskSearch()
        {
            return _tagContext
                    .TaskSearchResult
                    .AsQueryable<BsonModels.TaskSearch>()
                    .Select(x => x.Keyword)
                    .ToList();
        }

        private async Task<BsonModels.TaskSearch> GetTaskSearch(string keyword)
        {
            return await _tagContext
                .TaskSearchResult
                .Find(x => x.Keyword == keyword.ToLower())
                .FirstOrDefaultAsync();
        }
    }
}