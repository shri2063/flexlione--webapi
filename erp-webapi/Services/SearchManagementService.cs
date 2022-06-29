using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using flexli_erp_webapi.BsonModels;
using flexli_erp_webapi.DataModels;
using flexli_erp_webapi.EditModels;

namespace flexli_erp_webapi.Services
{
    public class SearchManagementService
    {
        public static List<TaskDetailEditModel> GetTaskListForSearchQuery(SearchQueryEditModel searchQuery)
        {

            List<TaskDetailEditModel> taskList = new List<TaskDetailEditModel>();
            if (searchQuery.Tag != null)
            {
                 SearchByTag(searchQuery).ForEach(x =>
                 {
                     taskList.Add(GetTaskDetailFromTaskSearchView(x));
                 });
            }
            else
            {
               SearchFromDb(searchQuery).ForEach(x =>
                   taskList.Add(TaskManagementService.GetTaskById(x.TaskId)));
            }

            return taskList;

        }

        private static TaskDetailEditModel GetTaskDetailFromTaskSearchView(TaskSearchView taskSearchView)
        {
          return new TaskDetailEditModel()
            {
                TaskId = taskSearchView.TaskId,
                AssignedTo = taskSearchView.AssignedTo,
                CreatedBy = taskSearchView.CreatedBy,
                Deadline = taskSearchView.Deadline,
                Description = taskSearchView.Description,
                Status = (EStatus) Enum.Parse(typeof(EStatus), taskSearchView.Status, true),
                 IsRemoved = taskSearchView.IsRemoved

            };
        }

        private static List<TaskSearchView> SearchByTag(SearchQueryEditModel searchQuery)
        {
            List<TaskSearchView> searchTaskList = TagManagementService
                .GetSearchTagResult("description", searchQuery.Tag)
                .Tasks
                .ToList();
            
            
            if (searchQuery.TaskId != null)
            {
                searchTaskList = searchTaskList.Where(y =>
                        y.TaskId == searchQuery.TaskId).ToList();
                
            }
            
            if (searchQuery.CreatedBy != null)
            {
               
                    searchTaskList = searchTaskList.Where(y =>
                        searchQuery.CreatedBy.Contains(y.CreatedBy)).ToList();
               
            }
            
            if (searchQuery.AssignedTo != null)
            {
                searchTaskList = searchTaskList.Where(y =>
                    searchQuery.AssignedTo.Contains(y.AssignedTo)).ToList();
            }
            
            if (searchQuery.Deadline != null)
            {
                searchTaskList = searchTaskList
                    .Where(x => x.Deadline <= searchQuery.Deadline)
                    .ToList();
            }
            
            

            if (searchQuery.Status != null)
            {
                
                    searchTaskList = searchTaskList.Where(y =>
                        searchQuery.CreatedBy.Contains(y.Status))
                        .ToList();
               
            }

            return searchTaskList;
        }
        
        public static List<TaskDetail> SearchFromDb(SearchQueryEditModel searchQuery)
        {
            using (var db = new ErpContext())
            {
                
                var query = (IQueryable<TaskDetail>) db.TaskDetail;
                if (searchQuery.Description != null)
                {
                    query = query.Where(row => row.Description.ToLower().Contains(searchQuery.Description.ToLower()));

                }
                if (searchQuery.CreatedBy != null)
                {
                    query = query.Where(row => searchQuery.CreatedBy.Contains(row.CreatedBy.ToLower()));
                }
                
                if (searchQuery.AssignedTo != null)
                {
                    query = query.Where(row => searchQuery.AssignedTo.Contains(row.AssignedTo.ToLower()));
                }
                
                if (searchQuery.Status != null)
                {
                    query = query.Where(row => searchQuery.Status.Contains(row.Status.ToLower()));
                }
                
                if (searchQuery.Deadline != null)
                {
                    query = query.Where(row => row.Deadline <= searchQuery.Deadline);
                }
                
                if (searchQuery.TaskId != null)
                {
                    query = query.Where(row => row.TaskId == searchQuery.TaskId);
                }
                
                if (searchQuery.IncludeRemoved == false)
                {
                    query = query.Where(row => row.IsRemoved != true);
                }
                return query.ToList();
            }

           

        }
    }
}