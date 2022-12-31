using System;
using System.Collections.Generic;
using System.Linq;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Policy.SearchPolicy.TemplateSearch.Interfaces;
using flexli_erp_webapi.Repository.Interfaces;
using flexli_erp_webapi.Services.TaskSearch;


namespace flexli_erp_webapi.Services
{
    public class AutoSearchByTagCompilerService
    {
        private readonly ITemplateTagSearchResultRepository _templateTagSearchResultRepository;
        private readonly IIgnoreSearchWordRepository _ignoreSearchWordRepository;
        private readonly ITaskTagSearchResultRepository _taskTagSearchResultRepository;
        private readonly ITaskRepository _taskRepository;
        private readonly ITemplateRepository _templateRepository;

        public AutoSearchByTagCompilerService(ITemplateTagSearchResultRepository templateTagSearchResultRepository,
            IIgnoreSearchWordRepository ignoreSearchWordRepository,
            ITaskTagSearchResultRepository taskTagSearchResultRepository,
            ITaskRepository taskRepository, ITemplateRepository templateRepository)
        {
          
            _templateTagSearchResultRepository = templateTagSearchResultRepository;
            _ignoreSearchWordRepository = ignoreSearchWordRepository;
            _taskTagSearchResultRepository = taskTagSearchResultRepository;
            _taskRepository = taskRepository;
            _templateRepository = templateRepository;
        }

      
       // Note : This API needs to be called only inititally to fill up past data 
        public void PopulatePreviousSearchResult(EAssignmentType type)
        {
            if (type == EAssignmentType.Task)
            {
                var taskList = _taskRepository.GetTaskIdList();
                taskList.ForEach(x => AddToSearchResults(x.Description, x.TaskId,EAssignmentType.Task));
            }
            if (type == EAssignmentType.Template)
            {
                var templateList = _templateRepository.GetAllTemplates();
                templateList.ForEach(x => AddToSearchResults(x.Description, x.TemplateId,EAssignmentType.Template));
            }
         
            
            
        }
        
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="templateId"></param>
        /// <param name="description"></param>
        public async void RemoveFromSearchResults(string Id, EAssignmentType type)
        {
            // for template
            if (type == EAssignmentType.Template)
            {
                // Get all template tags
                var allTags = await _templateTagSearchResultRepository.GetListOfTemplateTags();

                // for each corresponding tag templateList, remove template from list
                foreach (var tag in allTags)
                {
                    _templateTagSearchResultRepository.RemoveTemplateFromTemplateListOfTag(tag, Id);
                }

                return;
            }

            var allTaskTags = await _taskTagSearchResultRepository.GetListOfTaskTags();

            foreach (var tag in allTaskTags)
            {
                _taskTagSearchResultRepository.RemoveTaskFromTaskListOfTag(tag, Id);
            }
        }

        public  void AddToSearchResults(string description, string Id, EAssignmentType type)
        {
            if (description == null)
            {
                return;
            }
            // generate tag list from template description
            List<string> tags = description
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

            // In case of templates
            if (type == EAssignmentType.Template)
            {
                // Add description tags
                foreach (var tag in legalTags)
                {
                    _templateTagSearchResultRepository.AddTemplateToTemplateListOfTag(tag, Id);
                }

                return;
            }
            
            // In case of Tasks
            // Add description tags
            foreach (var tag in legalTags)
            {
                _taskTagSearchResultRepository.AddTaskToTaskListOfTag(tag, Id);
            }
        }
    }
}