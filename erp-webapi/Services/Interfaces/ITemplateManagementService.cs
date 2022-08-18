using System.Collections.Generic;
using flexli_erp_webapi.EditModels;

namespace flexli_erp_webapi.Services.Interfaces
{
    public interface ITemplateManagementService
    {
        TemplateEditModel GetTemplateById(string templateId, List<string> include = null);
        
        List<TemplateEditModel> GetSimilarTemplateList(string templateId = null);
        TemplateEditModel CreateOrUpdateTemplate(TemplateEditModel template);

        void DeleteTemplate(string templateId);

        TemplateEditModel CloneTemplate(string templateId);

        TemplateEditModel ReplaceChildTemplate(string oldTemplateId, string newTemplateId, string parentTemplateId);
        
        TemplateEditModel AddChildTemplate(string childTemplateId, string parentTemplateId);
        
        TemplateEditModel RemoveChildTemplate(string childTemplateId, string parentTemplateId);






    }
}