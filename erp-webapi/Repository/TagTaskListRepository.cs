using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using flexli_erp_webapi.BsonModels;
using flexli_erp_webapi.DataLayer.Interface;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Repository.Interfaces;
using flexli_erp_webapi.Services;
using MongoDB.Driver;
using Tag = flexli_erp_webapi.BsonModels.Tag;

namespace flexli_erp_webapi.Repository
{
    public class TagTaskListRepository: ITagTaskListRepository
    {
       
        private readonly ITagContext _tagContext;
        
        public TagTaskListRepository (ITagContext tagContext, ITagRepository tagRepository)
        {
            _tagContext = tagContext ?? throw new ArgumentNullException(nameof(tagContext));
        }

        public async Task<TagTaskList> GetTagTaskListForTag(string keyword, ETagType tagType)
        {

            return  await  _tagContext
                .TagTaskList
                .Find(x => x.Keyword == keyword.ToLower() && x.Type == tagType)
                .FirstOrDefaultAsync();
        }

        public async  Task<TagTaskList> CreateTagTaskListForTag( string keyword, ETagType tagType)
        {

            TagTaskList newTagTaskList = new TagTaskList()
            {
                
                Keyword = keyword,
                // In Case of Search Tag: populate all tasks containing search word
                Tasks = tagType == ETagType.SearchTag? AddTaskListForSearchTag(keyword):new List<TaskSearchView>(),
                Type = tagType
            };
            // [dbCheck]: Unique constraint on keyword + tagType
            await _tagContext.TagTaskList.InsertOneAsync(newTagTaskList);
            return await GetTagTaskListForTag(keyword, tagType);
        }

        public async Task<TagTaskList> AddTaskToTagTask(string keyword, List<string> taskIdList, ETagType tagType)
        {
           
           
            TagTaskList existingTagTask = await GetTagTaskListForTag(keyword.ToLower(), tagType);
            
            // [Check]: Tag has already been added in TagTask DB
            if (existingTagTask == null)
            {
                throw new KeyNotFoundException("Tag:  " + keyword + " has not been added to tagtask table");
            }

           
            List<string> existingTaskIds = (from s in  existingTagTask.Tasks
                select s.TaskId).ToList();
            List<TaskSearchView> filteredTaskListToAdd = new List<TaskSearchView>();
            
            
            foreach (var taskId in taskIdList)
            {
                // [Check] Task id is not present in existing task list
                if (!existingTaskIds.Contains(taskId))
                {
                    filteredTaskListToAdd.Add(GetTaskSearchViewForTask(TaskManagementService.GetTaskById(taskId)));   
                }
               
            }
            
            var update = Builders<TagTaskList>.Update
                .PushEach<TaskSearchView>(e => e.Tasks,filteredTaskListToAdd );
            var filter = Builders<TagTaskList>.Filter.Eq(e => e.Keyword, keyword.ToLower()) 
                         & Builders<TagTaskList>.Filter.Eq(e => e.Type, tagType) ;

            await _tagContext.TagTaskList.FindOneAndUpdateAsync(filter, update);

            return await GetTagTaskListForTag(keyword, tagType);
        }
        
        public async Task<TagTaskList> RemoveSearchTaskListFromTag(string keyword,ETagType tagType, string taskId =  null)
        {
            if (taskId == null)
            {
                return await RemoveAllSearchTaskListFromTag(keyword, tagType);
            }
            
            var filter = Builders<TagTaskList>.Filter.Eq(e => e.Keyword, keyword.ToLower()) 
                         & Builders<TagTaskList>.Filter.Eq(e => e.Type, tagType) ;

            var update = Builders<TagTaskList>.Update
                .PullFilter(e => e.Tasks, Builders<TaskSearchView>.Filter.Where(task => task.TaskId ==  taskId));

            await _tagContext.TagTaskList.FindOneAndUpdateAsync(filter, update);

            return await GetTagTaskListForTag(keyword, tagType);
        }

        public async Task<bool> DeleteTagTaskList(string keyword, ETagType tagType)
        {
            FilterDefinition<TagTaskList> filterForTagTaskList = Builders<TagTaskList>.Filter.Eq(e => e.Keyword, keyword.ToLower()) 
                                                                 & Builders<TagTaskList>.Filter.Eq(e => e.Type, tagType) ;

            DeleteResult deleteTaskTagList = await _tagContext
                .TagTaskList
                .DeleteOneAsync(filterForTagTaskList);

            if (deleteTaskTagList.IsAcknowledged)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> ParseTaskDescriptionForSearchTags(string description, string taskId,  IEnumerable<Tag> tagList)
        {
            IEnumerable<Tag> filteredTagList = tagList.ToList().FindAll(x => true == description.ToLower()
                .Contains(x.Keyword));
            foreach (var tag in filteredTagList)
            {
                // [action] remove earlier task details before updating new one
                await RemoveSearchTaskListFromTag(tag.Keyword, ETagType.SearchTag, taskId);
                await AddTaskToTagTask(tag.Keyword, new List<string>() { taskId }, ETagType.SearchTag);
            }

            return true;
            
        }

        public async Task<TagTaskList> ReviseTaskListForSearchTag(string keyword)
        {
            TagTaskList existingTagTask = await GetTagTaskListForTag(keyword.ToLower(), ETagType.SearchTag);
            
            // [Check]: Tag has already been added in TagTask DB
            if (existingTagTask == null)
            {
                throw new KeyNotFoundException("Tag:  " + keyword + " has not been added to tagtask table");
            }
            
            var filter = Builders<TagTaskList>.Filter.Eq(e => e.Keyword, keyword.ToLower()) 
                         & Builders<TagTaskList>.Filter.Eq(e => e.Type, ETagType.SearchTag) ;

            
            var arrayUpdate = Builders<TagTaskList>.Update.Set("tasks", AddTaskListForSearchTag(keyword));
          
            await _tagContext.TagTaskList.UpdateOneAsync(filter , arrayUpdate); 

            return await GetTagTaskListForTag(keyword, ETagType.SearchTag);
        }


        private async Task<TagTaskList> RemoveAllSearchTaskListFromTag(string keyword, ETagType tagType)
        {
            var filter = Builders<TagTaskList>.Filter.Eq(e => e.Keyword, keyword.ToLower()) 
                         & Builders<TagTaskList>.Filter.Eq(e => e.Type, tagType) ;

            
            var arrayUpdate = Builders<TagTaskList>.Update.Set("tasks", new List<TaskSearchView>());
          
            await _tagContext.TagTaskList. UpdateOneAsync(filter , arrayUpdate); 

            return await this.GetTagTaskListForTag(keyword, tagType);
        }

        

        
        
       
        private static List<TaskSearchView> AddTaskListForSearchTag (string description)
        {
            List<string> taskIds = (from s in TaskManagementService.GetTaskIdList()
                select s.TaskId).ToList();
            if (taskIds.Count == 0)
            {
                return null;
            }
            List<TaskDetailEditModel> taskList = new List<TaskDetailEditModel>();
            List<TaskSearchView> taskListContainingSearch = new List<TaskSearchView>();
            foreach (var taskId in taskIds)
            {
                taskList.Add(TaskManagementService.GetTaskById(taskId));
            }

            foreach (var task in taskList)
            {
                if (task.Description.ToLower().Contains(description.ToLower()))
                {
                    taskListContainingSearch.Add(GetTaskSearchViewForTask(task));
                }   
            }

            return taskListContainingSearch;

        }

        private static TaskSearchView GetTaskSearchViewForTask(TaskDetailEditModel task)
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
    }
    
}