using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using flexli_erp_webapi.BsonModels;
using flexli_erp_webapi.DataLayer.Interface;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Repository.Interfaces;
using flexli_erp_webapi.Services.Interfaces;
using flexli_erp_webapi.Services.TaskSearch;
using MongoDB.Driver;

namespace flexli_erp_webapi.Services.SearchPolicy
{
    
    public class SearchPriorityByCommonalityPolicy : ISearchPriorityPolicy
    {
        
        private readonly ITaskTagSearchResultRepository _taskTagSearchResultRepository;
        private readonly ITemplateTagContext _templateTagContext;

        public SearchPriorityByCommonalityPolicy(
            ITaskTagSearchResultRepository taskTagSearchResultRepository,
            ITemplateTagContext templateTagContext)
        {
            _taskTagSearchResultRepository = taskTagSearchResultRepository;
            _templateTagContext = templateTagContext;
        }

        public List<TemplateSearchView> getSearchResultForTemplateTags(List<string> tags)
        {
            List<TagTemplateList> listOfTagTemplateLists = new List<TagTemplateList>();

            // for each tag in query, find out tag-template-list
            foreach (var tag in tags)
            {
                listOfTagTemplateLists.AddRange(GetTemplateListForTagSearch(tag)
                    .GetAwaiter()
                    .GetResult());
            }

            if (listOfTagTemplateLists.Count == 0)
            {
                return new List<TemplateSearchView>();
            }
            
            List<TemplateSearchView> templates = new List<TemplateSearchView>();
            listOfTagTemplateLists.ForEach(x => templates.AddRange(x.Templates));
            var groupedCommonList = templates.GroupBy(x => x.TemplateId)
                .Select(x => new
                {
                    count = x.Count(),
                    template = x.ToList()
                })
                .OrderByDescending(x => x.count)
                .ThenByDescending(x => x.template[0].CreatedAt)
                .Select(x => x.template.FirstOrDefault() as TemplateSearchView).ToList();

            return groupedCommonList;
        }

       

        public List<TaskSearchView> getSearchResultForTaskTags(List<string> tags)
        {
            List<TaskSearchView> taskList = new List<TaskSearchView>();

            // for each tag in query, find out tag-template-list
            foreach (var tag in tags)
            {
                taskList.AddRange(GetTaskListForTagSearch(tag)
                    .GetAwaiter()
                    .GetResult()
                    .Distinct());
            }

            if (taskList.Count == 0)
            {
                return new List<TaskSearchView>();
            }

        
           
            var groupedCommonList = taskList
                .GroupBy(x => x.TaskId)
                .Select(x => new
                {
                    count = x.Count(),
                    task = x.ToList()
                })
                .OrderByDescending(x => x.count)
                .ThenByDescending(x => x.task[0].EditedAt)
                .Select(x => x.task.FirstOrDefault() as TaskSearchView).ToList();

            return groupedCommonList;
        }
        
        public async Task<List<TagTemplateList>> GetTemplateListForTagSearch(string keyword)
        {
            // returns templates of tags which are contained by given keyword
            var tagTemplateList = _templateTagContext
                .TemplateTagSearchResult
                .AsQueryable<TagTemplateList>()
                .Select(x => x)
                .ToList();

            List<TagTemplateList> filteredList = new List<TagTemplateList>();
            
            tagTemplateList.ForEach(x =>
            {
                if (keyword.Contains(x.Keyword))
                {
                    filteredList.Add(x);
                }
            });

            return filteredList;
        }

        private async Task<List<TaskSearchView>> GetTaskListForTagSearch(string keyword)
        {

           // Priority 1 - We found a perfect Match
            List<TaskSearchView> taskList = new List<TaskSearchView>();
            taskList = await _taskTagSearchResultRepository
                .GetTaskListForTag(keyword);

           // Priority 2 - We found a keyword that contains the given keyword
            if (taskList.Count() == 0)
            {
                var tagList = await _taskTagSearchResultRepository
                    .GetListOfTaskTags();
                
                tagList.ForEach(x => 
                {
                    if (x.Contains(keyword))
                    {
                        taskList.AddRange( _taskTagSearchResultRepository
                            .GetTaskListForTag(x)
                            .GetAwaiter()
                            .GetResult()
                            .ToList()
                            .OrderByDescending(y => y.EditedAt));
                    }
                });
            }
            
            // Priority 3 - We found a keyword that is contained in the given keyword
            if (taskList.Count() == 0)
            {
                var tagList = await _taskTagSearchResultRepository
                    .GetListOfTaskTags();
                
                tagList.ForEach(x => 
                {
                    if (keyword.Contains(x))
                    {
                        taskList.AddRange( _taskTagSearchResultRepository
                            .GetTaskListForTag(x)
                            .GetAwaiter()
                            .GetResult()
                            .ToList()
                            .OrderByDescending(y => y.EditedAt));
                    }
                });
            }
            return taskList;
        }
    }
}