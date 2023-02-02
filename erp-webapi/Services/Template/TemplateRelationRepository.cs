using System;
using System.Collections.Generic;
using System.Linq;
using flexli_erp_webapi;
using flexli_erp_webapi.DataModels;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Repository.Interfaces;

namespace m_sort_server
{
    public class TemplateRelationRepository: ITemplateRelationRepository
    {
       
        
        
        public List<TemplateRelationEditModel> GetTemplateRelationsByTemplateId(string templateId, string include)
        {
            using (var db = new ErpContext())
            {
                List<string> templateRelationIds = new List<string>();
                List<TemplateRelationEditModel> templateRelations = new List<TemplateRelationEditModel>();
                if(include.Contains("children"))
                {
                     templateRelationIds =  db.TemplateRelation
                        .Where(x => x.ParentTemplateId == templateId)
                        .Select(x => x.TemplateRelationId)
                        .ToList();

                }
                else if (include.Contains("parent"))
                {
                    templateRelationIds =  db.TemplateRelation
                        .Where(x => x.TemplateId == templateId)
                        .Select(x => x.TemplateRelationId)
                        .ToList();
                }
                
                templateRelationIds.ForEach(x => templateRelations.Add(GetTemplateRelationByRelationId(x)));
              

                return templateRelations;
            }
        }

        public TemplateRelationEditModel AddOrUpdateTemplateRelation(string templateId, string parentTemplateId)
        {
            using (var db = new ErpContext())
            {

                TemplateRelation existingTemplateRelation = db.TemplateRelation
                    .FirstOrDefault(x => x.TemplateId == templateId && x.ParentTemplateId == parentTemplateId);

                if (existingTemplateRelation != null)
                {
                    return null;
                }

              

                TemplateRelation templateRelation = new TemplateRelation()
                {
                    TemplateRelationId = GetNextAvailableIdForTemplateRelation(),
                    TemplateId = templateId,
                    ParentTemplateId = parentTemplateId
                };
                db.TemplateRelation.Add(templateRelation);
                db.SaveChanges();

                return GetTemplateRelationByRelationId(templateRelation.TemplateRelationId);
            }
           
        }

        public bool RemoveTemplateRelation(string templateId, string parentTemplateId)
        {
            TemplateRelation templateRelation;

            using (var db = new ErpContext())
            {
                templateRelation = db.TemplateRelation
                    .FirstOrDefault(x => x.TemplateId == templateId && x.ParentTemplateId == parentTemplateId);
                templateRelation.IfNotNull(x => db.TemplateRelation.Remove(templateRelation));
                db.SaveChanges();

            }

            return true;
        }
        
        private static string GetNextAvailableIdForTemplateRelation()
        {
            using (var db = new ErpContext())
            {
                var a = db.TemplateRelation
                    .Select(x => Convert.ToInt32(x.TemplateRelationId))
                    .DefaultIfEmpty(0)
                    .Max();
                return Convert.ToString(a + 1);
            }

        }

        private TemplateRelationEditModel GetTemplateRelationByRelationId(string templateRelationId)
        {
            using (var db = new ErpContext())
            {

                TemplateRelation existingTemplateRelation = db.TemplateRelation
                    .FirstOrDefault(x => x.TemplateRelationId == templateRelationId);

                if (existingTemplateRelation == null)
                    return null;

                // Case: In case you have to update data received from db

                TemplateRelationEditModel templateRelationEditModel = new TemplateRelationEditModel()
                {
                    TemplateRelationId = existingTemplateRelation.TemplateRelationId,
                    TemplateId = existingTemplateRelation.TemplateId,
                    ParentTemplateId = existingTemplateRelation.ParentTemplateId
                };
                return templateRelationEditModel;
            }
        }
    }
}