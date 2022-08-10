using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using flexli_erp_webapi.DataModels;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Repository.Interfaces;

namespace flexli_erp_webapi.Repository
{
   
    public static class Get {
        public static T IfNotNull<U ,T>(this U item, Func<U, T> lambda) where U: class {
            if (item == null) {
                return default(T);
            }
            return lambda(item);
        }
    }
    
    
    public class TemplateRepository: ITemplateRepository
    {
        

        ///<Summary>
        /// ToDo
        ///</Summary>
        public  TemplateEditModel GetTemplateById(string templateId)
        {
            using (var db = new ErpContext())
            {

                Template existingTemplate =  db.Template
                    .FirstOrDefault(x => x.TemplateId == templateId);

                if (existingTemplate == null)
                    return null;

                // Case: In case you have to update data received from db

                TemplateEditModel templateEditModel = new TemplateEditModel()
                {
                    TemplateId = existingTemplate.TemplateId,
                    Description = existingTemplate.Description,
                    CreatedAt = existingTemplate.CreatedAt,
                    Owner = existingTemplate.Owner,
                    ChildTemplateIds = existingTemplate.ChildTemplateIds?? new List<string>(),
                    CloneTemplateId = existingTemplate.CloneTemplateId
                };

                return templateEditModel;
            }

        }
        
        ///<Summary>
        /// ToDo
        ///</Summary>
        public  List<TemplateEditModel> GetAllTemplates()
        {
            List<TemplateEditModel> templates = new List<TemplateEditModel>();
            List<string> templateIds;

            using (var db = new ErpContext())
            {
                templateIds = db.Template
                    .Select(x => x.TemplateId)
                    .ToList();
            }

            templateIds.ForEach(x => { templates.Add(GetTemplateById(x)); });

            return templates;
        }
        
        ///<Summary>
        /// ToDo
        ///</Summary>
        public  List<TemplateEditModel> GetChildrenTemplates(string templateId)
        {
            List<TemplateEditModel> childTemplates = new List<TemplateEditModel>();
            List<string> childTemplateIds = GetTemplateById(templateId).ChildTemplateIds;

            if (childTemplateIds.Count == 0)
            {
                return childTemplates;
            }
            childTemplateIds.ForEach(x =>  childTemplates.Add(GetTemplateById(x)));
            return childTemplates;
        }

        ///<Summary>
        /// ToDo
        ///</Summary>
        public TemplateEditModel CreateOrUpdateTemplate(TemplateEditModel templateEditModel)
        {
            Template template;

            using (var db = new ErpContext())
            {
                template = db.Template
                    .FirstOrDefault(x => x.TemplateId == templateEditModel.TemplateId);

                // [Check] Cannot update Clone template Id
                if (template != null) // update
                {
                    template.TemplateId = templateEditModel.TemplateId;
                    template.Description = templateEditModel.Description;
                    template.Owner = templateEditModel.Owner;
                    template.ChildTemplateIds = templateEditModel.ChildTemplateIds;
                        db.SaveChanges();
                }
                else
                {
                    template = new Template()
                    {
                        TemplateId = GetNextAvailableIdForTemplate(),
                        Description = templateEditModel.Description,
                        Owner = templateEditModel.Owner,
                        ChildTemplateIds = templateEditModel.ChildTemplateIds,
                        CloneTemplateId = templateEditModel.CloneTemplateId,
                        CreatedAt = DateTime.Now
                    };
                    db.Template.Add(template);
                    db.SaveChanges();
                }
            }

            return GetTemplateById(template.TemplateId);
        }
        ///<Summary>
        /// ToDo
        ///</Summary>
        public bool DeleteTemplate(string templateId)
        {
            Template template;

            using (var db = new ErpContext())
            {
                template = db.Template
                    .FirstOrDefault(x => x.TemplateId == templateId);
                template.IfNotNull(x => db.Template.Remove(template));
                db.SaveChanges();

            }

            return true;
        }

        private static string GetNextAvailableIdForTemplate()
        {
            using (var db = new ErpContext())
            {
                var a = db.Template
                    .Select(x => Convert.ToInt32(x.TemplateId))
                    .DefaultIfEmpty(0)
                    .Max();
                return Convert.ToString(a + 1);
            }

        }

       
    }

}