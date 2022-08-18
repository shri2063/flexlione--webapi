using System;
using System.Collections.Generic;
using System.Linq;
using flexli_erp_webapi.DataModels;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Repository;
using flexli_erp_webapi.Repository.Interfaces;
using flexli_erp_webapi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace flexli_erp_webapi.Services
{
    ///<Summary>
    /// ToDo
    ///</Summary>
    public class TemplateManagementService: ITemplateManagementService
    {

        private readonly ITemplateRepository _templateRepository;
        
        ///<Summary>
        /// ToDo
        ///</Summary>
        public TemplateManagementService(ITemplateRepository templateRepository)
        {
            _templateRepository = templateRepository;
        }

        ///<Summary>
        /// ToDo
        ///</Summary>
        public TemplateEditModel GetTemplateById(string templateId, List<string> include = null)
        {
            TemplateEditModel templateEditModel =_templateRepository.GetTemplateById(templateId);
            if (include.IfNotNull(x => x.Count) == 0)
            {
                return templateEditModel;
            }

            if (include.Contains("child"))
            {
                templateEditModel.ChildTemplates = _templateRepository.GetChildrenTemplates(templateId);
            }
            return templateEditModel;
        }
        ///<Summary>
        /// ToDo
        ///</Summary>

        public List<TemplateEditModel> GetSimilarTemplateList(string templateId = null)
        {
            List<TemplateEditModel> templateList =  _templateRepository.GetAllTemplates();

            if (templateId == null)
            {
                return templateList;
            }

            var selectedTemplate = _templateRepository.GetTemplateById(templateId);
            return templateList.FindAll(x => x.CloneTemplateId == selectedTemplate.CloneTemplateId);
        }

        ///<Summary>
        /// ToDo
        ///</Summary>
        public  void DeleteTemplate(string templateId)
        {
           
            TemplateEditModel existingTemplate = _templateRepository.GetTemplateById(templateId);
            if (existingTemplate == null)
            {
                return;
            }
            // [check] : If Template has any parent template
            if (existingTemplate.ChildTemplateIds.Count > 0)
            {
                throw new KeyNotFoundException("One or more child templates exist: " + 
                                               JsonSerializer.Serialize(existingTemplate.ChildTemplateIds));
            }
            
            // [check] : If it serves as a clone template
            if (existingTemplate.CloneTemplateId == null)
            {
                throw new KeyNotFoundException("Serves as a clone template ");
            }
            _templateRepository.DeleteTemplate(templateId);
        }
        
        ///<Summary>
        /// ToDo
        ///</Summary>
        public  TemplateEditModel CreateOrUpdateTemplate(TemplateEditModel templateIdEditModel)
        {
           return _templateRepository.CreateOrUpdateTemplate(templateIdEditModel);
        }
        
        ///<Summary>
        /// ToDo
        ///</Summary>
        public  TemplateEditModel CloneTemplate(string templateId)
        {
            TemplateEditModel currentTemplateEditModel = _templateRepository.GetTemplateById(templateId);

            var cloneTemplateEditModel = currentTemplateEditModel ?? throw new KeyNotFoundException(
                "Selected model for clone does not exist: "
                + templateId);
            
            cloneTemplateEditModel.TemplateId = "newTemplate";
            cloneTemplateEditModel.CloneTemplateId = currentTemplateEditModel.CloneTemplateId;

            return _templateRepository.CreateOrUpdateTemplate(cloneTemplateEditModel);
        }
        
        
        ///<Summary>
        /// ToDo
        ///</Summary>
        public  TemplateEditModel ReplaceChildTemplate(string oldTemplateId, string newTemplateId, string parentTemplateId)
        {
            TemplateEditModel parentTemplateEditModel = _templateRepository.GetTemplateById(parentTemplateId);

            //[check] : Parent template id exists
            var checkParentTemplate = parentTemplateEditModel ?? throw new KeyNotFoundException(
                "Selected model for parent does not exist: "
                + parentTemplateId);
            
            TemplateEditModel oldTemplateEditModel = _templateRepository.GetTemplateById(oldTemplateId);
            
            //[Check] : Old template id exists
            var checkOldTemplate = oldTemplateEditModel ?? throw new KeyNotFoundException(
                "Selected model for old template does not exist: "
                + parentTemplateId);
            
            
            TemplateEditModel newTemplateEditModel = _templateRepository.GetTemplateById(newTemplateId);
            //[Check] : new template id exists
            var checkNewTemplate = newTemplateEditModel ?? throw new KeyNotFoundException(
                "Selected model for old template does not exist: "
                + parentTemplateId);

            parentTemplateEditModel.ChildTemplateIds =
                (from s in _templateRepository.GetChildrenTemplates(parentTemplateId)
                    select s.TemplateId).ToList();

            parentTemplateEditModel.ChildTemplateIds.Remove(oldTemplateId);
            parentTemplateEditModel.ChildTemplateIds.Add(newTemplateId);

            return _templateRepository.CreateOrUpdateTemplate(parentTemplateEditModel);
        }

        public TemplateEditModel AddChildTemplate(string childTemplateId, string parentTemplateId)
        {
            TemplateEditModel parentTemplateEditModel = _templateRepository.GetTemplateById(parentTemplateId);

            // [Check]: If Parent template exists
            var parentTemplateExists = parentTemplateEditModel ?? throw new KeyNotFoundException(
                "Selected model for parent does not exist: "
                + parentTemplateId);
            
            TemplateEditModel childTemplateEditModel = _templateRepository.GetTemplateById(childTemplateId);

            // [Check]: If Child template exists
            var childTemplateExists = childTemplateEditModel ?? throw new KeyNotFoundException(
                "Selected model for child does not exist: "
                + childTemplateId);

            parentTemplateEditModel.ChildTemplateIds.Add(childTemplateId);

            return _templateRepository.CreateOrUpdateTemplate(parentTemplateEditModel);
        }
        
        public TemplateEditModel RemoveChildTemplate(string childTemplateId, string parentTemplateId)
        {
            TemplateEditModel parentTemplateEditModel = _templateRepository.GetTemplateById(parentTemplateId);

            // [Check]: If Parent template exists
            var parentTemplateExists = parentTemplateEditModel ?? throw new KeyNotFoundException(
                "Selected model for parent does not exist: "
                + parentTemplateId);
            
            TemplateEditModel childTemplateEditModel = _templateRepository.GetTemplateById(childTemplateId);

           

            parentTemplateEditModel.ChildTemplateIds.Remove(childTemplateId);

            return _templateRepository.CreateOrUpdateTemplate(parentTemplateEditModel);
        }
    }
}