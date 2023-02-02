using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using flexli_erp_webapi.BsonModels;
using flexli_erp_webapi.DataLayer.Interface;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Repository.Interfaces;
using flexli_erp_webapi.Services;
using MongoDB.Driver;

namespace flexli_erp_webapi.Repository
{
    public class LabelRelationRepository : ILabelRelationRepository
    {
        private readonly ITagContext _tagContext;
        private readonly ITaskRepository _taskRepository;

        public LabelRelationRepository(ITagContext tagContext, ITaskRepository taskRepository)
        {
            _tagContext = tagContext;
            _taskRepository = taskRepository;
        }

        public async Task<List<TaskSearchView>> GetSprintLabelTaskForProfileId(string profileId)
        {
            var list = await _tagContext
                .SprintTasks
                .Find(x => x.Task.AssignedTo == profileId)
                .ToListAsync();

            return list.Select(x => x.Task).ToList();
        }

        public async Task<List<TaskSearchView>> GetNotCompleteLabelTaskForProfileId(string profileId)
        {
            using (var db = new ErpContext())
            {
                var notCompleteTask = db.TaskDetail
                    .Where(x => x.Status != EStatus.completed.ToString() && x.CreatedBy == profileId)
                    .Select(x => x.TaskId)
                    .ToList();

                List<TaskSearchView> notCompletedLabelTask = new List<TaskSearchView>();
                        
                notCompleteTask.ForEach(taskId =>
                {
                    notCompletedLabelTask.Add(GetTaskSearchViewForTask(_taskRepository.GetTaskById(taskId)));
                });

                return notCompletedLabelTask;
            }
        }

        public async Task<SprintLabelTask> AddSprintLabelToTask(string taskId)
        {
            TaskDetailEditModel task = _taskRepository.GetTaskById(taskId);
            

            //[Check]: If task exist
            if (task == null)
            {
                throw new ArgumentException("Task don't exist for given taskId");
            }
            
            var existingSprintTask = await GetSprintTask(taskId);
            
            if (existingSprintTask != null)
            {
                throw new KeyNotFoundException("Task already added");
            }
            
            // create new tasksearchview object
            TaskSearchView newTaskView = new TaskSearchView()
            {
                TaskId = task.TaskId,
                AssignedTo = task.AssignedTo,
                Status = task.Status.ToString(),
                Description = task.Description,
                CreatedAt = task.CreatedAt,
                EditedAt = task.EditedAt
            };

            SprintLabelTask newSprintLabelTask = new SprintLabelTask()
            {
                Task = newTaskView
            };
            
            // add into mongoDb database
            await _tagContext.SprintTasks.InsertOneAsync(newSprintLabelTask);
            
            // check if successfully created
            var createdSprintTask = await GetSprintTask(newTaskView.TaskId);
            
            if (createdSprintTask == null)
            {
                throw new KeyNotFoundException("Task not added");
            }

            return createdSprintTask;
        }

        // local function to get document with given taskId
        private async Task<SprintLabelTask> GetSprintTask(string taskId)
        {
            return await _tagContext
                .SprintTasks
                .Find(x => x.Task.TaskId == taskId)
                .SingleOrDefaultAsync();
        }
        
        private static TaskSearchView GetTaskSearchViewForTask(TaskDetailEditModel task)
        {
            return new TaskSearchView()
            {
                TaskId = task.TaskId,
                Description = task.Description,
                CreatedBy = task.CreatedBy,
                AssignedTo = task.AssignedTo,
                Deadline = task.Deadline,
                Status = task.Status.ToString(),
                IsRemoved = task.IsRemoved
            };
        }
    }
}