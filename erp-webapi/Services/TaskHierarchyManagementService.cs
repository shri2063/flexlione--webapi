using System.Collections.Generic;
using System.Linq;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Repository.Interfaces;
using flexli_erp_webapi.Services;
using m_sort_server.Repository.Interfaces;

namespace flexli_erp_webapi.Services
{
    public class TaskHierarchyManagementService
    {

        private readonly ITaskHierarchyRelationRepository _taskHierarchyRelationRepository;
        private readonly ITaskRepository _taskRepository;
        private readonly ITaskHourCalculatorHandler _taskHourCalculatorHandler;
        public TaskHierarchyManagementService(ITaskHierarchyRelationRepository taskHierarchyRelationRepository, 
             ITaskRepository taskRepository, ITaskHourCalculatorHandler taskHourCalculatorHandler)
        {
            _taskHierarchyRelationRepository = taskHierarchyRelationRepository;
            _taskRepository = taskRepository;
            _taskHourCalculatorHandler = taskHourCalculatorHandler;
        }
        public  TaskHierarchyEditModel GetTaskHierarchyByTaskId(string taskId, string include = null)
        {
            TaskHierarchyEditModel taskHierarchy = _taskHierarchyRelationRepository.GetTaskHierarchyByTaskIdFromDb(taskId);
            taskHierarchy.TotalHoursSpent = _taskHourCalculatorHandler.GetTotalActualHoursForTask(taskId);
            taskHierarchy.TotalEstimatedHours = _taskHourCalculatorHandler.GetTotalEstimatedHoursForTask(taskId);
            
            if (include == null)
            {
                return taskHierarchy;
            }

            if (include.Contains("children"))
            {
                taskHierarchy.ChildrenTaskHierarchy = new List<TaskHierarchyEditModel>();
                var childTaskIds = _taskRepository.GetChildTaskIdList(taskId);
                    childTaskIds.ForEach(x =>taskHierarchy.ChildrenTaskHierarchy
                        .Add(GetTaskHierarchyByTaskId(x)));
                return taskHierarchy;
            }

            return taskHierarchy;

        }
    }
}