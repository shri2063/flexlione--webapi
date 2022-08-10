using System.Collections.Generic;
using System.Threading.Tasks;
using flexli_erp_webapi.BsonModels;
using flexli_erp_webapi.EditModels;

namespace flexli_erp_webapi.Repository.Interfaces
{
    public interface ITemplateRepository
    {
        TemplateEditModel GetTemplateById(string templateId);

        List<TemplateEditModel> GetAllTemplates();

        List<TemplateEditModel> GetChildrenTemplates(string templateId);
        TemplateEditModel CreateOrUpdateTemplate(TemplateEditModel templateEditModel);
        
        bool DeleteTemplate(string templateId);
    }
}