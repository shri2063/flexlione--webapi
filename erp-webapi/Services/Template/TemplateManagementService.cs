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
using m_sort_server;

namespace flexli_erp_webapi.Services
{
    ///<Summary>
    /// ToDo
    ///</Summary>
    public class TemplateManagementService: ITemplateManagementService
    {

        private readonly ITemplateRepository _templateRepository;
        private readonly ITemplateRelationRepository _templateRelationRepository;
        
        ///<Summary>
        /// ToDo
        ///</Summary>
        public TemplateManagementService(ITemplateRepository templateRepository, ITemplateRelationRepository templateRelationRepository)
        {
            _templateRepository = templateRepository;
            _templateRelationRepository = templateRelationRepository;
        }

        ///<Summary>
        /// ToDo
        ///</Summary>
        public TemplateEditModel GetTemplateById(string templateId, List<string> include = null)
        {
            TemplateEditModel templateEditModel =_templateRepository.GetTemplateById(templateId);
            if (include == null || include.IfNotNull(x => x.Count) == 0)
            {
                return templateEditModel;
            }

            if (include.Contains("children"))
            {
                templateEditModel.ChildTemplates = GetChildrenForTemplateId(templateId);
            }
            
            if (include.Contains("parent"))
            {
                templateEditModel.ParentTemplates = GetParentForTemplateId(templateId);
            }
            return templateEditModel;
        }

        ///<Summary>
        /// ToDo
        ///</Summary>

       

        public List<TemplateEditModel> GetSimilarTemplateList(string templateId = null, int? pageIndex = null, int? pageSize = null)
        {
            List<TemplateEditModel> templateList =  _templateRepository.GetAllTemplates();

            if (templateId == null)
            {
                return templateList;
            }

            var selectedTemplate = _templateRepository.GetTemplateById(templateId);

            //[check]: Pagination Check
            if (pageIndex != null && pageSize != null)
            {
                return GetSimilarTemplateListPage(templateList, selectedTemplate.CloneTemplateId, (int) pageIndex, (int) pageSize);
            }
            return templateList.FindAll(x => x.CloneTemplateId == selectedTemplate.CloneTemplateId);
        }

        private List<TemplateEditModel> GetSimilarTemplateListPage(List<TemplateEditModel> templateList, string selectedTemplateCloneTemplateId, int pageIndex, int pageSize)
        {
            if (pageIndex <= 0 || pageSize <= 0)
                throw new ArgumentException("Incorrect value for pageIndex or pageSize");
                
            // skip take logic
            List<TemplateEditModel> filteredTemplateList = templateList
                .FindAll(x=>x.CloneTemplateId == selectedTemplateCloneTemplateId)
                .OrderByDescending(t=>Convert.ToInt32(t.TemplateId))
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();;

            if (filteredTemplateList.Count == 0)
            {
                throw new ArgumentException("Incorrect value for pageIndex or pageSize");
            }

            return filteredTemplateList;
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
            if (GetChildrenForTemplateId(templateId).Count > 0)
            {
                throw new KeyNotFoundException("One or more child templates exist: ");
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
            //[check] :  template id exists
            CheckTemplateExists(templateId);
            TemplateEditModel currentTemplateEditModel = _templateRepository.GetTemplateById(templateId);
            var cloneTemplateEditModel = currentTemplateEditModel;
            cloneTemplateEditModel.TemplateId = "newTemplate";
            cloneTemplateEditModel.CloneTemplateId = cloneTemplateEditModel.CloneTemplateId.IfNull(cloneTemplateId => templateId);
            return _templateRepository.CreateOrUpdateTemplate(cloneTemplateEditModel);
        }
        
        
        ///<Summary>
        /// ToDo
        ///</Summary>
        public  TemplateEditModel ReplaceChildTemplate(string oldTemplateId, string newTemplateId, string parentTemplateId)
        {
            //[check] :  template id exists
            CheckTemplateExists(parentTemplateId);
            CheckTemplateExists(newTemplateId); 
            CheckTemplateExists(oldTemplateId);
            
            _templateRelationRepository.RemoveTemplateRelation(oldTemplateId, parentTemplateId);
            _templateRelationRepository.AddOrUpdateTemplateRelation(newTemplateId, parentTemplateId);
            return  GetTemplateById(parentTemplateId,include("children"));
        }

     

        public TemplateEditModel AddChildTemplate(string childTemplateId, string parentTemplateId)
        {
            //[check] :  template id exists
            CheckTemplateExists(parentTemplateId);
            CheckTemplateExists(childTemplateId);

            _templateRelationRepository.AddOrUpdateTemplateRelation(childTemplateId, parentTemplateId);

            return GetTemplateById(parentTemplateId,include("children"));
        }
        
        public TemplateEditModel RemoveChildTemplate(string childTemplateId, string parentTemplateId)
        {

            //[check] :  template id exists
            CheckTemplateExists(parentTemplateId);
            CheckTemplateExists(childTemplateId);

            _templateRelationRepository.RemoveTemplateRelation(childTemplateId, parentTemplateId);

            return GetTemplateById(parentTemplateId,include("children"));
        }
        
        private bool CheckTemplateExists(string templateId)
        {
            return _templateRepository.GetTemplateById(templateId) != null ? true : false;
        }

        private List<TemplateEditModel> GetChildrenForTemplateId(string templateId)
        {
            List<TemplateEditModel> templates = new List<TemplateEditModel>();
            var childTemplateIds = _templateRelationRepository
                .GetTemplateRelationsByTemplateId(templateId, "children")
                .IfNotNull(y => y.Select(x => x.TemplateId).ToList());

            if (childTemplateIds == null)
            {
                return templates;
            }
            foreach (var childTemplateId in childTemplateIds)
            {
                templates.Add(GetTemplateById(childTemplateId));
            }

            return templates;
        }
        
        private List<TemplateEditModel> GetParentForTemplateId(string templateId)
        {
            List<TemplateEditModel> templates = new List<TemplateEditModel>();
            var parentTemplateIds = _templateRelationRepository
                .GetTemplateRelationsByTemplateId(templateId, "parent")
                .IfNotNull(y => y.Select(x => x.ParentTemplateId).ToList());

            if (parentTemplateIds == null)
            {
                return templates;
            }
            foreach (var parentTemplateId in parentTemplateIds)
            {
                templates.Add(GetTemplateById(parentTemplateId));
            }

            return templates;
        }
        
        private   List<string> include(string include)
        {
            List<string> includeList = new List<string>();
            includeList.Add(include);
            return includeList;
        }

        
    }
}