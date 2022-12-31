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
    public class TaskTagSearchResultRepository : ITaskTagSearchResultRepository
    {
        private readonly ITagContext _tagContext;
        private readonly ITaskRepository _taskRepository;

        public TaskTagSearchResultRepository(ITagContext tagContext, ITaskRepository taskRepository)
        {
            _tagContext = tagContext ?? throw new ArgumentNullException(nameof(tagContext));
            _taskRepository = taskRepository;
        }

        public async Task<List<TaskSearchView>> GetTaskListForTag(string keyword)
        {
           var tasktag =  await _tagContext
                .TaskTagSearchResult
                .Find(x => x.Keyword == keyword.ToLower())
                .FirstOrDefaultAsync();
           if (tasktag == null)
           {
               return new List<TaskSearchView>();
           }
           return tasktag.Tasks.ToList();
        }

        public async Task<List<TaskSearchView>> AddTaskToTaskListOfTag(string keyword, string taskId)
        {
            TaskTag tasktag = await GetTaskTag(keyword.ToLower());
            
            // [Check]: Tag has already been added in TagTask DB
            if (tasktag == null )
            {
                tasktag =  await CreateTag(keyword);
              
            }

            var existingTaskIds = (from k in tasktag.Tasks select k.TaskId).ToList();

            if (!existingTaskIds.Contains(taskId))
            {
                var update = Builders<TaskTag>.Update
                    .Push<TaskSearchView>(e => e.Tasks,
                        GetTaskSearchViewForTask(_taskRepository.GetTaskById(taskId)));
                
                var filter = Builders<TaskTag>.Filter.Eq(e => e.Keyword, keyword.ToLower() );
                
                await _tagContext.TaskTagSearchResult.FindOneAndUpdateAsync(filter, update);
            }

            return await GetTaskListForTag(keyword);
        }

        private TaskSearchView GetTaskSearchViewForTask(TaskDetailEditModel task)
        {
            return new TaskSearchView()
            {
                TaskId = task.TaskId,
                Description = task.Description,
                CreatedBy = task.CreatedBy,
                AssignedTo = task.AssignedTo,
                Deadline = task.Deadline,
                Status = task.Status.ToString(),
                IsRemoved = task.IsRemoved
            };
        }

        public async  Task<TaskTag> CreateTag(string keyword)
        {
            TaskTag newTaskTag = new TaskTag()
            {
                
                Keyword = keyword,
                // In Case of Search Tag: populate all tasks containing search word
                Tasks = new List<TaskSearchView>()
            };
            
            // [dbCheck]: Unique constraint on keyword + tagType
            await _tagContext.TaskTagSearchResult.InsertOneAsync(newTaskTag);

            return await GetTaskTag(keyword);
        }

       
       

        public async Task<List<TaskSearchView>> RemoveTaskFromTaskListOfTag(string keyword, string taskId)
        {
            var filter = Builders<TaskTag>.Filter.Eq(e => e.Keyword, keyword.ToLower()) ;

            var update = Builders<TaskTag>.Update
                .PullFilter(e => e.Tasks, Builders<TaskSearchView>.Filter.Where(task => task.TaskId ==  taskId));

            await _tagContext.TaskTagSearchResult.FindOneAndUpdateAsync(filter, update);

            return await GetTaskListForTag(keyword);
        }

        public async Task<List<string>> GetListOfTaskTags()
        {
            return _tagContext
                    .TaskTagSearchResult
                    .AsQueryable<TaskTag>()
                    .Select(x => x.Keyword)
                    .ToList();
        }

        private async Task<TaskTag> GetTaskTag(string keyword)
        {
            return await _tagContext
                .TaskTagSearchResult
                .Find(x => x.Keyword == keyword.ToLower())
                .FirstOrDefaultAsync();
        }
    }
}