using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using flexli_erp_webapi.Controller;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Repository.Interfaces;
using m_sort_server;
using Microsoft.EntityFrameworkCore.Internal;

namespace flexli_erp_webapi.Services
{
    public class TemplateMainService:TemplateManagementService
    {

        private readonly ITaskRepository _taskRepository;
        private readonly IDictionary<string, string> _taskTemplateMapping = new Dictionary<string, string>();
        private  IDictionary<string, string> _roleProfileMapping = new Dictionary<string, string>();


        public TemplateMainService(ITemplateRepository templateRepository, ITemplateRelationRepository templateRelationRepository, ITaskRepository taskRepository, TaskSearchResultRelationRepository taskSearchResultRelationRepository) : base(templateRepository, templateRelationRepository)
        {
            _taskRepository = taskRepository;
        }
       

        ///<Summary>
        /// ToDo
        ///</Summary>
        public List<TaskDetailEditModel> GenerateMultipleTaskFromTemplate(TaskTemplateEditModel taskTemplate)
        {
            _taskTemplateMapping.Clear();
            // Mapping Parent task id with a dummy template with Id "head"
            _taskTemplateMapping.Add("head",taskTemplate.ParentTaskId);
            _roleProfileMapping = taskTemplate.RoleProfileMap;
            List<string> templateIds = new List<string>();
            List<TaskDetailEditModel> createdTasks = new List<TaskDetailEditModel>();
          
            // Include reference template id
            if (taskTemplate.IncludeReference)
            {
                templateIds.Add(taskTemplate.TemplateId);
            }

            var childTemplateIds = taskTemplate.IncludeAllChildren
                ? GetExhaustiveChildList(taskTemplate.TemplateId)
                : GetImmediateChildList(taskTemplate.TemplateId);
         
            templateIds.AddRange(childTemplateIds);
            
            // [Data Consistency] In case of any error in creating task, move state back to initial before creating tasks
            try
            {
                templateIds.ForEach(x =>
                {
                    createdTasks.Add(CreateTaskFromTemplateId(x, taskTemplate.CreatedBy));
                });
            }
            catch
            {
                createdTasks.ForEach(y => _taskRepository.DeleteTask(y.TaskId));
                throw new KeyNotFoundException("Tasks not created");
            }
            
            return createdTasks;

        }

      
        public List<string> GetAllRolesForTemplateId(string templateId, List<string> include =  null)
        {
            List<string> templateIds = new List<string>();
            List<string> roles = new List<string>();
            templateIds.Add(templateId);
            if (include == null)
            {
                templateIds.AddRange(GetImmediateChildList(templateId));
            }
            else if(include.Contains("allChildren"))

            {
                templateIds.AddRange(GetExhaustiveChildList(templateId));
            }
            
            templateIds.AddRange(GetExhaustiveChildList(templateId));
            templateIds.ForEach(x =>
            {
                roles.Add(GetRoleForTemplateId(x));
            });

            return roles.Distinct().ToList();

        }

        private string GetRoleForTemplateId(string templateId)
        {
            return GetTemplateById(templateId).Role;
        }

        private string GetProfileForRole(string role)
        {

            return _roleProfileMapping[_roleProfileMapping.Keys.FirstOrDefault(x => x == role)
                                ?? throw new KeyNotFoundException("role does not exist")];
        }
        
        private List<string> GetImmediateChildList(string templateId)
        {
            List<string> templateIdList = new List<string>();
            GetTemplateById(templateId, include("children"))
                .ChildTemplates
                .ForEach(x => templateIdList.Add(x.TemplateId));

            return templateIdList;
        }

        private List<string> GetExhaustiveChildList(string templateId)
        {
            SingleLinkList<string> templateIdLinkList = new SingleLinkList<string>();
            templateIdLinkList.Add(templateId);
            while (templateIdLinkList.Pointer() != null)
            {
                GetTemplateById(templateIdLinkList.Pointer(),include("children"))
                    .ChildTemplates
                    .ForEach(x =>   templateIdLinkList.Add(x.TemplateId));
               templateIdLinkList.Next();
            }

            var templateIdList = templateIdLinkList.GetList();
            templateIdLinkList.Clear();
            templateIdList.RemoveAt(0);
            return templateIdList;
        }

       
        private TaskDetailEditModel CreateTaskFromTemplateId(string templateId, string createdBy)
        {
            TemplateEditModel template = GetTemplateById(templateId,include("parent"));

            
            TaskDetailEditModel task = new TaskDetailEditModel()
            {
                TaskId = "server-generated",
                ParentTaskId = GetParentTaskIdForTemplate(template.TemplateId),
                AssignedTo = GetProfileForRole(template.Role).IfNull(x => createdBy),
                CreatedBy = createdBy,
                Description = template.Description,
                CreatedAt = DateTime.Now,
                Deadline = DateTime.MaxValue,
                EditedAt = DateTime.Now,
                Status = EStatus.yettostart
            };

            TaskDetailEditModel createdTask = _taskRepository.CreateOrUpdateTask(task);
            _taskTemplateMapping.Add(templateId,createdTask.TaskId);

            return createdTask;

        }

        private string GetParentTaskIdForTemplate(string templateId)
        {
            TemplateEditModel template = GetTemplateById(templateId, include("parent"));
            var key = _taskTemplateMapping.Keys.FirstOrDefault(x =>
            {
                return template.ParentTemplates
                    .Select(y => y.TemplateId)
                    .Contains(x);
            });

            if (key == null)
            {
                return _taskTemplateMapping["head"];
            }
            return _taskTemplateMapping[key];
        }


        private   List<string> include(string include)
        {
            List<string> includeList = new List<string>();
            includeList.Add(include);
            return includeList;
        }


        public TemplateEditModel AddOrUpdateTemplate(TemplateEditModel template)
        {
            TemplateEditModel existingTemplate = GetTemplateById(template.TemplateId);

            // [Check]: if template exist already then remove it from all tags
            if (existingTemplate != null)
            {
                // Todo Make Template Search responsive to Add or update template
                //_searchResultRelationRepository.RemoveFromSearchResults(template.TemplateId, EAssignmentType.Template);
            }
            
            // Call CreateUpdate function of management service
            var crudTemplate = CreateOrUpdateTemplate(template);
            
            // Todo Make Template Search responsive to Add or update template
            // tagging of template description
           // _searchResultRelationRepository.AddToSearchResults(crudTemplate.Description, crudTemplate.TemplateId, EAssignmentType.Template);
            
            return crudTemplate;
        }
    }
}