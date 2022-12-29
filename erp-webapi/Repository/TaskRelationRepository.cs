using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using flexli_erp_webapi.DataModels;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Services;
using Microsoft.EntityFrameworkCore;

namespace flexli_erp_webapi.Repository.Interfaces
{
    public class TaskRelationRepository : ITaskRelationRepository
    {

        private readonly ISprintRepository _sprintRepository;
        private readonly ITaskRepository _taskRepository;

        public TaskRelationRepository(ISprintRepository sprintRepository, ITaskRepository taskRepository)
        {
            _sprintRepository = sprintRepository;
            _taskRepository = taskRepository;
        }

        public List<string> GetTaskIdsForSprint(string sprintId)
        {
            List<string> taskIds = new List<string>();

            var sprint = _sprintRepository.GetSprintById(sprintId);
            if (sprint == null)
            {
                return taskIds;
            }

            var planningStage = new List<SStatus> { SStatus.Planning, SStatus.RequestForApproval };
            if (planningStage.Contains(sprint.Status))
            {

                using (var db = new ErpContext())
                {

                    taskIds = db.TaskDetail
                        .Where(x => x.SprintId == sprintId)
                        .Select(y => y.TaskId)
                        .ToList();
                }

                return taskIds;

            }

            using (var db = new ErpContext())
            {

                taskIds = db.SprintReport
                    .Where(x => x.SprintId == sprintId)
                    .Select(y => y.TaskId)
                    .Distinct()
                    .ToList();
            }

            return taskIds;
        }


        public TaskDetailEditModel LinkTaskToSprint(string taskId, string sprintId)
        {
            using (var db = new ErpContext())
            {
                TaskDetail existingTask = db.TaskDetail
                    .FirstOrDefault(x => x.TaskId == taskId);

                //[Check]: Task does not exist
                if (existingTask == null)
                    throw new KeyNotFoundException("TaskDetail does not exist");

                // [Check]: Task is not linked to some other sprint
                if (existingTask.SprintId != null)
                {
                    throw new ConstraintException("task already link to sprint" + existingTask.SprintId);
                }

                Sprint sprint = db.Sprint
                    .FirstOrDefault(x => x.SprintId == sprintId);
                //[Check]: Sprint does not exist
                if (sprint == null)
                    throw new KeyNotFoundException("Sprint does not exist");

                // [Check]: Sprint is in planning stage
                if (sprint.Status != SStatus.Planning.ToString())
                {
                    throw new ConstraintException("cannot link task to sprint as sprint is not in planning stage");
                }

                existingTask.SprintId = sprintId;
                db.SaveChanges();

                return  _taskRepository.GetTaskById(existingTask.TaskId);
            }

        }

        public  TaskDetailEditModel RemoveTaskFromSprint(string taskId)
        {
            using (var db = new ErpContext())
            {
                TaskDetail existingTask = db.TaskDetail
                    .Include(x => x.TaskSchedules)
                    .FirstOrDefault(x => x.TaskId == taskId);

                // Case: TaskDetail does not exist
                if (existingTask == null)
                    throw new KeyNotFoundException("TaskDetail does not exist");



                Sprint sprint = db.Sprint
                    .FirstOrDefault(x => x.SprintId == existingTask.SprintId);

                // Case : Task Not link to any sprint
                if (sprint == null)
                {
                    return _taskRepository.GetTaskById(existingTask.TaskId);
                }

                // Case: Sprint is already approved
                if (sprint.Approved && !sprint.Closed)
                {
                    throw new ConstraintException("cannot delete the task as sprint is already approved");
                }

                existingTask.SprintId = null;
                db.SaveChanges();

                return  _taskRepository.GetTaskById(existingTask.TaskId);
            }

        }
    }


}