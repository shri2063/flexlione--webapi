using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using flexli_erp_webapi.BsonModels;
using flexli_erp_webapi.DataLayer.Interface;
using flexli_erp_webapi.Policy.SearchPolicy.TemplateSearch.Interfaces;
using flexli_erp_webapi.Repository.Interfaces;
using MongoDB.Driver;

namespace flexli_erp_webapi.Services
{
    public class AutoSearchByTagCompiler
    {
        private readonly ITemplateTagSearchResultRepository _templateTagSearchResultRepository;
        private readonly ITemplateTagContext _templateTagContext;
        private readonly IIgnoreSearchWordRepository _ignoreSearchWordRepository;

        public AutoSearchByTagCompiler(ITemplateTagSearchResultRepository templateTagSearchResultRepository,
            ITemplateTagContext templateTagContext,
            IIgnoreSearchWordRepository ignoreSearchWordRepository)
        {
          
            _templateTagSearchResultRepository = templateTagSearchResultRepository;
            _templateTagContext = templateTagContext;
            _ignoreSearchWordRepository = ignoreSearchWordRepository;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="templateId"></param>
        /// <param name="description"></param>
        public async void RemoveFromSearchResults(string templateId)
        {
            // Get all template tags
            var allTags = await _templateTagSearchResultRepository.GetListOfTemplateTags();

            // for each corresponding tag templateList, remove template from list
            foreach (var tag in allTags)
            {
                _templateTagSearchResultRepository.RemoveTemplateFromTemplateListOfTag(tag, templateId);
            }
        }

        public async void AddToSearchResults(string description, string templateId)
        {
            // generate tag list from template description
            List<string> templateTags = description
                .ToLower()
                .Split(' ')
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
            var legalTemplateTags = templateTags.Except(ignoreSearchWords).ToList().Distinct();

            // Add description tags
            foreach (var tag in legalTemplateTags)
            {
                _templateTagSearchResultRepository.AddTemplateToTemplateListOfTag(tag, templateId);
            }
        }
    }
}