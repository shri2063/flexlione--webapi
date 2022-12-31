using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using flexli_erp_webapi.BsonModels;
using flexli_erp_webapi.DataModels;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Repository;
using flexli_erp_webapi.Repository.Interfaces;
using flexli_erp_webapi.Services.Interfaces;

namespace flexli_erp_webapi.Services
{
    public class TaskSearchManagementService
    {
        private readonly ISearchPriorityPolicy _searchPriorityByCommonalityPolicy;
        private readonly ITaskRepository _taskRepository;

        public TaskSearchManagementService(
            ISearchPriorityPolicy searchPriorityPolicy, ITaskRepository taskRepository)
        {
            _searchPriorityByCommonalityPolicy = searchPriorityPolicy;
            _taskRepository = taskRepository;
        }


       
        
        public List<TaskDetailEditModel> GetTaskListForSearchQuery(SearchQueryEditModel searchQuery)
        {

            List<TaskDetailEditModel> taskList = new List<TaskDetailEditModel>();

            SearchFromDb(searchQuery).ForEach(x =>
                taskList.Add(_taskRepository.GetTaskById(x.TaskId)));


            return taskList;

        }



        public static List<TaskDetail> SearchFromDb(SearchQueryEditModel searchQuery)
        {
            using (var db = new ErpContext())
            {

                var query = (IQueryable<TaskDetail>)db.TaskDetail;
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

                query = query.OrderByDescending(t => t.CreatedAt);
                return query.Skip(0).Take(25).ToList();

            }



        }

        public List<TaskDetailEditModel> GetTaskListForSearchQuery(string searchQuery,
            int? pageIndex = null, int? pageSize = null)
        {
            // convert query into list of tags in lower case
            List<string> taskTags = searchQuery
                .ToLower()
                .Split(' ')
                .ToList();

            // get searched templates
            var tasks = _searchPriorityByCommonalityPolicy.getSearchResultForTaskTags(taskTags);

            List<TaskDetailEditModel> taskList = new List<TaskDetailEditModel>();

            // for each template-search-view to templateEditModel
            tasks.ForEach(task => { taskList.Add(GetTaskDetailFromTaskSearchView(task)); });

            // Pagination
            if (pageIndex != null && pageSize != null)
            {
                if (pageIndex <= 0 || pageSize <= 0)
                    throw new ArgumentException("Incorrect value for pageIndex or pageSize");

                var pageTaskList = taskList
                    .OrderByDescending(x => x.EditedAt)
                    .Skip((int)((pageIndex - 1) * pageSize))
                    .Take((int)pageSize)
                    .ToList();

                if (pageTaskList.Count == 0)
                {
                    throw new ArgumentException("Sorry, Did not find any matching Tasks result for search query:  " + searchQuery);
                }

                return pageTaskList;
            }

            return taskList.OrderByDescending(x => x.EditedAt).ToList();
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
                Status = (EStatus)Enum.Parse(typeof(EStatus), taskSearchView.Status, true),
                IsRemoved = taskSearchView.IsRemoved

            };
        }
    }


}