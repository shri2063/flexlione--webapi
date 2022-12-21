using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using flexli_erp_webapi.BsonModels;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Repository.Interfaces;
using flexli_erp_webapi.Services.Interfaces;

namespace flexli_erp_webapi.Services.SearchPolicy
{
    public class SearchPriorityByCommonalityPolicy : ISearchPriorityPolicy
    {
        
        private readonly ITemplateTagSearchResultRepository _templateTagSearchResultRepository;

        public SearchPriorityByCommonalityPolicy(
            ITemplateTagSearchResultRepository templateTagSearchResultRepository)
        {
           
            _templateTagSearchResultRepository = templateTagSearchResultRepository;
        }

        public List<TemplateSearchView> getSearchResultForTags(List<string> tags)
        {
            List<TagTemplateList> listOfTagTemplateLists = new List<TagTemplateList>();

            // for each tag in query, find out tag-template-list
            foreach (var tag in tags)
            {
                listOfTagTemplateLists.Add(_templateTagSearchResultRepository.GetTemplateListForTag(tag)
                    .GetAwaiter()
                    .GetResult());
            }

            if (listOfTagTemplateLists.Count == 0)
            {
                return new List<TemplateSearchView>();
            }

            // from tag-template-list model, select only templates
            // List<List<TemplateSearchView>> listOfTemplateLists = new List<List<TemplateSearchView>>(from s in  listOfTagTemplateLists
            //   select s.Templates as List<TemplateSearchView>).ToList();

            List<TemplateSearchView> templates = new List<TemplateSearchView>();
            listOfTagTemplateLists.ForEach(x => templates.AddRange(x.Templates));
            var groupedCommonList = templates.GroupBy(x => x.TemplateId)
                .Select(x => new
                {
                    count = x.Count(),
                    template = x.ToList()
                })
                .OrderByDescending(x => x.count)
                .Select(x => x.template.FirstOrDefault() as TemplateSearchView).ToList();

            return groupedCommonList;
        }
    }
}