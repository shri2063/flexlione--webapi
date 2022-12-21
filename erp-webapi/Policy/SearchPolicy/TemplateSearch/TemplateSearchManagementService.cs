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

        public List<TemplateEditModel> GetTemplateListForSearchQuery(string searchQuery)
        {
            // convert query into list of tags in lower case
            List<string> templateTags = searchQuery
                .ToLower()
                .Split(' ')
                .ToList();

            // get searched templates
            var templates = _searchPriorityByCommonalityPolicy.getSearchResultForTags(templateTags);

            List<TemplateEditModel> templateList = new List<TemplateEditModel>();
            
            // for each template-search-view to templateEditModel
            templates.ForEach(template =>
            {
                templateList.Add(GetTemplateFromTemplateSearchView(template));
            });

            return templateList;
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