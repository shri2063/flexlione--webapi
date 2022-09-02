using System;
using System.Collections.Generic;
using System.Linq;
using flexli_erp_webapi.DataModels;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Repository.Interfaces;
using m_sort_server;

namespace flexli_erp_webapi.Repository
{
   
    
    
    
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
                    CloneTemplateId = existingTemplate.CloneTemplateId,
                    Role = existingTemplate.Role,
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
                    template.Description = templateEditModel.Description.IfNull(description => template.Description);
                    template.Owner = templateEditModel.Owner.IfNull(owner => template.Owner);
                    template.Role = templateEditModel.Role.IfNull(owner => template.Role);
                    db.SaveChanges();
                }
                else
                {
                    template = new Template()
                    {
                        TemplateId = GetNextAvailableIdForTemplate(),
                        Description = templateEditModel.Description,
                        Owner = templateEditModel.Owner,
                        CreatedAt = DateTime.Now,
                        Role = templateEditModel.Role
                    };
                    // If clone template not provided , template will act as first clone of itself
                    template.CloneTemplateId = templateEditModel.CloneTemplateId.IfNull(cloneTemplateId => GetNextAvailableIdForTemplate());
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