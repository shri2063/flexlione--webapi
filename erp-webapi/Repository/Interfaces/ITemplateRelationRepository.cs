using System.Collections.Generic;
using flexli_erp_webapi.EditModels;

namespace flexli_erp_webapi.Repository.Interfaces
{
    public interface ITemplateRelationRepository
    {
        
        List<TemplateRelationEditModel> GetTemplateRelationsByTemplateId(string templateId,string include);
        TemplateRelationEditModel AddOrUpdateTemplateRelation(string templateId, string parentTemplateId);
        bool RemoveTemplateRelation(string templateId, string parentTemplateId);
    }
}