using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using flexli_erp_webapi.BsonModels;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Policy.SearchPolicy.TemplateSearch.Interfaces;
using flexli_erp_webapi.Repository.Interfaces;
using flexli_erp_webapi.Services.TaskSearch;


namespace flexli_erp_webapi.Services
{
    public class TaskSearchResultRelationRepository
    {
        private readonly IIgnoreSearchWordRepository _ignoreSearchWordRepository;
        private readonly ITaskSearchResultRepository _taskSearchResultRepository;
        private readonly ITaskRepository _taskRepository;
        private readonly ITaskAnchorRepository _taskAnchorRepository;

        public TaskSearchResultRelationRepository(
            IIgnoreSearchWordRepository ignoreSearchWordRepository,
            ITaskSearchResultRepository taskSearchResultRepository,
            ITaskRepository taskRepository, ITaskAnchorRepository taskAnchorRepository)
        {
          
           
            _ignoreSearchWordRepository = ignoreSearchWordRepository;
            _taskSearchResultRepository = taskSearchResultRepository;
            _taskRepository = taskRepository;
            _taskAnchorRepository = taskAnchorRepository;
            
         
           
              
            
        }

      
       // Note : This API needs to be called only initially to fill up past data 
        public async void PopulatePreviousSearchResult(EAssignmentType type)
        {
            if (type == EAssignmentType.Task)
            {
                var taskList = _taskRepository.GetTaskIdList();
                foreach (var taskShortDetailEditModel in taskList)
                {
                    await  RemoveFromSearchResults(taskShortDetailEditModel.TaskId);
                    AddToSearchResults(_taskRepository.GetTaskById(taskShortDetailEditModel.TaskId)); 
                }
              
            }
        }

        public async void UpdateTaskSearchViews(TaskDetailEditModel updatedTask)
        {
            var searchResults = await _taskAnchorRepository.GetSearchResults(updatedTask.TaskId);
            searchResults.ForEach(x => _taskSearchResultRepository.AddOrUpdateTask(x.ToLower(),GetTaskSearchViewForTask(updatedTask)));  
        }
        
      
        
        
        
        public async Task<bool> RemoveFromSearchResults(string taskId)
        {
           
           var searchresults = await _taskAnchorRepository.GetSearchResults(taskId);
           if (searchresults!= null)
           {
               searchresults.ForEach(x => _taskSearchResultRepository.RemoveTask(x, taskId));
               
           }
           await _taskAnchorRepository.ReviseSearchResults(taskId, new List<string>());

           return true;
        }

        public  async Task<bool> AddToSearchResults(TaskDetailEditModel task)
        {
            if (task.Description == null)
            {
                return false;
            }
            // generate tag list from template description
            List<string> tags = task.Description
                .ToLower()
                .Split(new Char[] { ',', '\\', '\n' ,'-', ' '},
                    StringSplitOptions.RemoveEmptyEntries)
                .Distinct()
                .ToList();

            // generating stop words
            List<string> ignoreSearchWords = _ignoreSearchWordRepository
                .GetIgnoreSearchWordList()
                .GetAwaiter()
                .GetResult()
                .Select(x => x.Keyword)
                .Distinct()
                .ToList();

            // remove stop words
            var legalTags = tags.Except(ignoreSearchWords).ToList().Distinct();

           
            
            // In case of Tasks
            // Add description tags
            await _taskAnchorRepository.ReviseSearchResults(task.TaskId, legalTags.ToList());
            foreach (var tag in legalTags)
            {
               await _taskSearchResultRepository.AddOrUpdateTask(tag.ToLower(), GetTaskSearchViewForTask(task));
            }

            return true;
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
                IsRemoved = task.IsRemoved,
                CreatedAt = task.CreatedAt,
                EditedAt = task.EditedAt
            };
        }
    }
}