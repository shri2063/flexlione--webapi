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
        
        private readonly ITaskRankingRepository _taskRankingRepository;
        private readonly ITaskRepository _taskRepository;
        
        public TaskRankingManagementService( ITaskRankingRepository taskRankingRepository,ITaskRepository taskRepository)
        {
            _taskRankingRepository = taskRankingRepository;
            _taskRepository = taskRepository;


        }

        public async Task<List<TaskDetailEditModel>> GetChildTaskRankingForTask(String parentTaskId)
        {
            List<String> taskIds =  await _taskRankingRepository.GetChildTaskRanking(parentTaskId);
            List<TaskDetailEditModel> childTaskList = new List<TaskDetailEditModel>();

            if (taskIds.Count() == 0)
            {
                taskIds = await _taskRankingRepository.ReviseChildTaskRanking(parentTaskId,  _taskRepository.GetChildTaskIdList(parentTaskId)); 
            }
            
            taskIds.ForEach(x => childTaskList.Add(_taskRepository.GetTaskById(x)));
            return childTaskList;
        }

        public async Task<List<String>> UpdateRankingOfTask(TaskDetailEditModel task)
        {
            List<String> curentRanking = await _taskRankingRepository
                .GetChildTaskRanking(task.ParentTaskId);
            if (curentRanking.Count() == 0)
            {
                curentRanking = _taskRepository.GetChildTaskIdList(task.ParentTaskId);
            }
            LinkedList<String> linkedList = new LinkedList<string>(curentRanking);
            
            linkedList.Remove(task.TaskId);
            var lastNode =  linkedList.Find(task.PositionAfter);
           linkedList.AddAfter(lastNode, task.TaskId);
           return await _taskRankingRepository.ReviseChildTaskRanking(task.ParentTaskId, linkedList.ToArray().ToList());


        }
    }
}