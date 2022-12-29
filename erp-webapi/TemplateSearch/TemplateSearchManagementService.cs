using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using flexli_erp_webapi.BsonModels;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Repository.Interfaces;
using flexli_erp_webapi.Services.Interfaces;

namespace flexli_erp_webapi.Services
{
    public class TagSearchManagementService
    {
        private readonly ISearchPriorityPolicy _searchPriorityByCommonalityPolicy;

        public TagSearchManagementService(ISearchPriorityPolicy searchPriorityByCommonalityPolicy)
        {
            _searchPriorityByCommonalityPolicy = searchPriorityByCommonalityPolicy;
        }

        public List<TemplateEditModel> GetTemplateListForSearchQuery(string searchQuery,
            int? pageIndex = null, int? pageSize = null)
        {
            // convert query into list of tags in lower case
            List<string> templateTags = searchQuery
                .ToLower()
                .Split(' ')
                .ToList();

            // get searched templates
            var templates = _searchPriorityByCommonalityPolicy.getSearchResultForTemplateTags(templateTags);

            List<TemplateEditModel> templateList = new List<TemplateEditModel>();
            
            // for each template-search-view to templateEditModel
            templates.ForEach(template =>
            {
                templateList.Add(GetTemplateFromTemplateSearchView(template));
            });

            // Pagination
            if (pageIndex != null && pageSize != null)
            {
                if (pageIndex <= 0 || pageSize <= 0)
                    throw new ArgumentException("Incorrect value for pageIndex or pageSize");
                
                var pageTemplateList = templateList
                    .OrderByDescending(x => x.EditedAt)
                    .Skip((int)((pageIndex - 1) * pageSize))
                    .Take((int)pageSize)
                    .ToList();
                
                if (pageTemplateList.Count == 0)
                {
                    throw new ArgumentException("Sorry, Did not find any matching Templates result for search query:  " + searchQuery);
                }

                return pageTemplateList;
            }

            return templateList.OrderByDescending(x=>x.EditedAt).ToList();
        }

        private static TemplateEditModel GetTemplateFromTemplateSearchView(TemplateSearchView template)
        {
            return new TemplateEditModel()
            {
                TemplateId = template.TemplateId,
                Description = template.Description,
                Owner = template.Owner,
                CreatedAt = template.CreatedAt,
                CloneTemplateId = template.CloneTemplateId,
                Role = template.Role
            };
        }
    }
}