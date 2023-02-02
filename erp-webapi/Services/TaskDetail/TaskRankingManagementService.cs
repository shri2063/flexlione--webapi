using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Repository.Interfaces;
using flexli_erp_webapi.Services.Interfaces;

namespace flexli_erp_webapi.Services
{
    public class TaskRankingManagementService : ITaskRankingManagementService
    {
        
        private readonly ITaskAnchorRepository _taskAnchorRepository;
        private readonly ITaskRepository _taskRepository;
        
        public TaskRankingManagementService( ITaskAnchorRepository taskAnchorRepository,ITaskRepository taskRepository)
        {
            _taskAnchorRepository = taskAnchorRepository;
            _taskRepository = taskRepository;


        }

        public async Task<List<TaskDetailEditModel>> GetChildTaskRankingForTask(String parentTaskId)
        {
            List<String> taskIds =  await _taskAnchorRepository.GetChildTaskRanking(parentTaskId);
            List<TaskDetailEditModel> childTaskList = new List<TaskDetailEditModel>();

            if (taskIds == null)
            {
                taskIds = await _taskAnchorRepository.ReviseChildTaskRanking(parentTaskId,  _taskRepository.GetChildTaskIdList(parentTaskId)); 
            }
            
            taskIds.ForEach(x => childTaskList.Add(_taskRepository.GetTaskById(x)));
            return childTaskList;
        }

        public async Task<List<String>> UpdateRankingOfTask(TaskDetailEditModel task)
        {
            // Validate: Task cannot be positioned after itself
            if (task.TaskId == task.PositionAfter)
            {
                throw new KeyNotFoundException("Task cannot be positioned after itself");
            }
            List<String> curentRanking = await _taskAnchorRepository
                .GetChildTaskRanking(task.ParentTaskId);
            if (curentRanking.Count() == 0)
            {
                curentRanking = _taskRepository.GetChildTaskIdList(task.ParentTaskId);
            }
            LinkedList<String> linkedList = new LinkedList<string>(curentRanking);
            
            linkedList.Remove(task.TaskId);
            var lastNode =  linkedList.Find(task.PositionAfter);
            if (lastNode == null)
            {
                linkedList.AddAfter(linkedList.Last, task.TaskId);
            }
            else
            {
                linkedList.AddAfter(lastNode, task.TaskId);
            }
          
           return await _taskAnchorRepository.ReviseChildTaskRanking(task.ParentTaskId, linkedList.ToArray().ToList());


        }
        
        
        public async Task<List<String>> RemoveRankingOfTask(TaskDetailEditModel task)
        {
            List<String> curentRanking = await _taskAnchorRepository
                .GetChildTaskRanking(task.ParentTaskId);
            if (curentRanking.Count() == 0)
            {
                curentRanking = _taskRepository.GetChildTaskIdList(task.ParentTaskId);
            }
            LinkedList<String> linkedList = new LinkedList<string>(curentRanking);
            
            linkedList.Remove(task.TaskId);
          
            return await _taskAnchorRepository.ReviseChildTaskRanking(task.ParentTaskId, linkedList.ToArray().ToList());


        }
    }
}