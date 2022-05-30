using System;
using System.Collections.Generic;
using System.Linq;
using m_sort_server.DataModels;
using m_sort_server.EditModels;
using Microsoft.EntityFrameworkCore;

namespace m_sort_server.Services
{
    public class TemplateManagementService
    {
        public static TemplateEditModel GetTemplateById(string templateId, string include = null)
        {
            TemplateEditModel templateEditModel = GetTemplateByIdFromDb(templateId);
            if (include == null)
            {
                return templateEditModel;  
            }

            if (include.Contains("taskDetails"))
            {
                templateEditModel.TaskList = GetTasksForTemplateId(templateId);
            }

            return templateEditModel;
        }

        public static List<TemplateEditModel> GetAllTemplates(string include = null)
        {
            List<TemplateEditModel> templates = new List<TemplateEditModel>();
            List<string> templateIds;

            using (var db = new ErpContext())
            {
                templateIds = db.Template
                    .Select(x => x.TemplateId)
                    .ToList();
            }
            templateIds.ForEach(x =>
            {
                templates.Add(GetTemplateById(x));
            });

            return templates;
        }

        private static TemplateEditModel GetTemplateByIdFromDb (string templateId)
        {
            using (var db = new ErpContext())
            {
                
                Template existingTemplate= db.Template
                    .FirstOrDefault(x => x.TemplateId == templateId);
                
                if ( existingTemplate== null)
                    return null;
                
                // Case: In case you have to update data received from db

                TemplateEditModel templateEditModel = new TemplateEditModel()
                {
                   TemplateId =  existingTemplate.TemplateId,
                   Description = existingTemplate.Description
                };

                return templateEditModel;
            }

        }
        
         public static TemplateEditModel AddOrUpdateTemplate(TemplateEditModel templateEditModel)
        {
            return AddOrUpdateTemplateInDb(templateEditModel);

        }
        
        private static TemplateEditModel AddOrUpdateTemplateInDb(TemplateEditModel templateEditModel)
        {
            Template template;
            
            using (var db = new ErpContext())
            {
                 template = db.Template
                    .FirstOrDefault(x => x.TemplateId == templateEditModel.TemplateId);


                if (template != null) // update
                {
                    template.TemplateId = templateEditModel.TemplateId;
                    template.Description = templateEditModel.Description;
                    db.SaveChanges();
                }
                else
                {
                    template = new Template()
                    {
                        TemplateId = GetNextAvailableIdForTemplate(),
                        Description = templateEditModel.Description
                    };
                    db.Template.Add(template);
                    db.SaveChanges();
                }
            }

            return GetTemplateById(template.TemplateId);
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
        
        private static string GetNextAvailableIdForTemplateTask()
        {
            using (var db = new ErpContext())
            {
                var a = db.TemplateTask
                    .Select(x => Convert.ToInt32(x.TemplateTaskId))
                    .DefaultIfEmpty(0)
                    .Max();
                return Convert.ToString(a + 1);
            }
          
        }

        public static List<TaskDetailEditModel> GetTasksForTemplateId(string templateId)
        {
            List<TaskDetailEditModel> taskDetailList = new List<TaskDetailEditModel>();
            using (var db = new ErpContext())
            {
                List<TemplateTask> templateTasks = db.TemplateTask
                    .Where(x => x.TemplateId == templateId)
                    .ToList();

                if (templateTasks.Count == 0)
                {
                    throw  new Exception("Relation between template and tasks does not exist");
                }
                templateTasks.ForEach(x =>
               {
                   taskDetailList.Add(TaskManagementService.GetTaskById(x.TaskId)); 
               });
                
            }

            return taskDetailList;
        }

        public static TemplateEditModel AddTaskListToTemplate(List<string> taskIdList, string templateId)
        {
            taskIdList.ForEach(taskId =>
            {
                TemplateTask templateTask;
                using (var db = new ErpContext())
                {
                    templateTask = db.TemplateTask
                        .FirstOrDefault(x => x.TemplateId == templateId && x.TaskId == taskId);


                    if (templateTask != null) // update
                    {
                        templateTask.TemplateId = templateId;
                        templateTask.TaskId = taskId;
                        db.SaveChanges();
                    }
                    else
                    {
                        templateTask = new TemplateTask()
                        {
                            TemplateTaskId = GetNextAvailableIdForTemplateTask(),
                            TemplateId = templateId,
                            TaskId = taskId
                        };
                        db.TemplateTask.Add(templateTask);
                        db.SaveChanges();
                    }
                }

               
            });
            return GetTemplateById(templateId, "taskDetails");
        }
        
        public static void RemoveTaskListFromTemplate(List<string> taskIdList, string templateId)
        {
            taskIdList.ForEach(taskId =>
            {
                TemplateTask templateTask;
                using (var db = new ErpContext())
                {
                    templateTask = db.TemplateTask
                        .FirstOrDefault(x => x.TemplateId == templateId && x.TaskId == taskId);


                    if (templateTask == null) // update
                    {
                        return;
                    }

                    DeleteTemplateTask(templateTask.TemplateTaskId);
                }
            });
        }
        
        public static void DeleteTemplate(string templateId)
        {
            using (var db = new ErpContext())
            {
                // Get Selected Profile
                Template existingTemplate = db.Template
                    .FirstOrDefault(x => x.TemplateId == templateId);
                
                if (existingTemplate != null)
                {
                    
                    db.Template.Remove(existingTemplate);
                    db.SaveChanges();
                }
            }
        }
        
        public static void DeleteTemplateTask(string templateTaskId)
        {
            using (var db = new ErpContext())
            {
                // Get Selected Profile
                TemplateTask existingTemplateTask = db.TemplateTask
                    .FirstOrDefault(x => x.TemplateTaskId == templateTaskId);
                
                if (existingTemplateTask != null)
                {
                    
                    db.TemplateTask.Remove(existingTemplateTask);
                    db.SaveChanges();
                }
            }
        }
    }
}