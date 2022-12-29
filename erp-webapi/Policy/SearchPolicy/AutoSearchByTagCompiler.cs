using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using flexli_erp_webapi.BsonModels;
using flexli_erp_webapi.DataLayer.Interface;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Policy.SearchPolicy.TemplateSearch.Interfaces;
using flexli_erp_webapi.Repository.Interfaces;
using flexli_erp_webapi.Services.TaskSearch;
using MongoDB.Driver;

namespace flexli_erp_webapi.Services
{
    public class AutoSearchByTagCompiler
    {
        private readonly ITemplateTagSearchResultRepository _templateTagSearchResultRepository;
        private readonly IIgnoreSearchWordRepository _ignoreSearchWordRepository;
        private readonly ITaskTagSearchResultRepository _taskTagSearchResultRepository;

        public AutoSearchByTagCompiler(ITemplateTagSearchResultRepository templateTagSearchResultRepository,
            IIgnoreSearchWordRepository ignoreSearchWordRepository,
            ITaskTagSearchResultRepository taskTagSearchResultRepository)
        {
          
            _templateTagSearchResultRepository = templateTagSearchResultRepository;
            _ignoreSearchWordRepository = ignoreSearchWordRepository;
            _taskTagSearchResultRepository = taskTagSearchResultRepository;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="templateId"></param>
        /// <param name="description"></param>
        public async void RemoveFromSearchResults(string Id, ECheckListType type)
        {
            // for template
            if (type == ECheckListType.Template)
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

        public async void AddToSearchResults(string description, string Id, ECheckListType type)
        {
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
            if (type == ECheckListType.Template)
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