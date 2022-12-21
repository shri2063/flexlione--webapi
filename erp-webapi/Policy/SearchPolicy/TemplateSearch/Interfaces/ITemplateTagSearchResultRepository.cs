using System.Collections.Generic;
using System.Threading.Tasks;
using flexli_erp_webapi.BsonModels;

namespace flexli_erp_webapi.Repository.Interfaces
{
    public interface ITemplateTagSearchResultRepository
    {
        Task<TagTemplateList> GetTemplateListForTag(string keyword);


        Task<TagTemplateList> AddTemplateToTemplateListOfTag(string keyword, string templateId);

        Task<TagTemplateList> CreateTemplateListForTag(string keyword);

        Task<TagTemplateList> RemoveTemplateFromTemplateListOfTag(string keyword, string templateId);

        Task<List<string>> GetListOfTemplateTags();
    }






}