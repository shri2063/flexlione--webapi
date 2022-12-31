using System.Collections.Generic;
using System.Threading.Tasks;
using flexli_erp_webapi.BsonModels;
using flexli_erp_webapi.EditModels;

namespace flexli_erp_webapi.Services.Interfaces
{
    public interface ISearchPriorityPolicy
    {
        
        List<TemplateSearchView> getSearchResultForTemplateTags(List<string> tags);

        List<TaskSearchView> getSearchResultForTaskTags(List<string> tags);
        
       
    }
}