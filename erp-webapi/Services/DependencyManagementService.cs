using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using flexli_erp_webapi.DataModels;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Repository.Interfaces;
using mflexli_erp_webapi.Repository.Interfaces;

namespace flexli_erp_webapi.Services
{
    public class DependencyManagementService

    {
        private readonly ITaskRepository _taskRepository;
        private readonly IDependencyRepository _dependencyRepository;
        public DependencyManagementService(ITaskRepository taskRepository, IDependencyRepository dependencyRepository)
        {
            _taskRepository = taskRepository;
            _dependencyRepository = dependencyRepository;
        }


        public List<DependencyEditModel> GetUpstreamDependenciesByTaskId(string taskId, int? pageIndex = null,
            int? pageSize = null)
        {
            var dependencies = _dependencyRepository.GetUpstreamDependenciesByTaskId(taskId, pageIndex, pageSize);
            dependencies.ForEach(x => x.TaskDetailEditModel = _taskRepository.GetTaskById(x.TaskId));
            return dependencies;
        }
        
        public List<DependencyEditModel> GetDownstreamDependenciesByTaskId(string taskId, int? pageIndex = null,
            int? pageSize = null)
        {
            var dependencies = _dependencyRepository.GetDownstreamDependenciesByTaskId(taskId, pageIndex, pageSize);
            dependencies.ForEach(x => x.TaskDetailEditModel = _taskRepository.GetTaskById(x.TaskId));
            return dependencies;
        }
        
        private  DependencyEditModel GetDependencyById(string dependencyId)
        {

            DependencyEditModel dependencyEditModel;
            using (var db = new ErpContext())
            {

                dependencyEditModel = db.Dependency
                    .Where(x => x.DependencyId == dependencyId)
                    .Select(s => new DependencyEditModel()
                    {
                        DependencyId = s.DependencyId,
                        DependentTaskId = s.DependentTaskId,
                        Description = s.Description,
                        TaskId = s.TaskId
                    }).ToList().FirstOrDefault();
            }

            return dependencyEditModel;
        }
        
        public  DependencyEditModel CreateOrUpdateDependency(DependencyEditModel dependencyEditModel)
        {
            Dependency dependency;
            
            using (var db = new ErpContext())
            {
                dependency = db.Dependency
                    .FirstOrDefault(x => x.DependencyId == dependencyEditModel.DependencyId);


                if (dependency != null) // update
                {
                    dependency.TaskId = dependencyEditModel.TaskId;
                    dependency.Description = dependencyEditModel.Description;
                    dependency.DependentTaskId = dependencyEditModel.DependentTaskId;

                    db.SaveChanges();
                }
                else
                {
                    dependency = new Dependency()
                    {
                        DependencyId = GetNextAvailableDependencyId(),
                        TaskId = dependencyEditModel.TaskId,
                        Description = dependencyEditModel.Description,
                        DependentTaskId = dependencyEditModel.DependentTaskId,
                    };
                    db.Dependency.Add(dependency);
                    db.SaveChanges();
                }
            }

            return GetDependencyById(dependency.DependencyId);
        }
        
        private  string GetNextAvailableDependencyId()
        {
            using (var db = new ErpContext())
            {
                var a = db.Dependency
                    .Select(x => Convert.ToInt32(x.DependencyId))
                    .DefaultIfEmpty(0)
                    .Max();
                return Convert.ToString(a + 1);
            }
          
        }
        
        public  void DeleteDependency(string dependencyId)
        {
            using (var db = new ErpContext())
            {
                // Get Selected Dependency
                Dependency existingDependency = db.Dependency
                    .FirstOrDefault(x => x.DependencyId == dependencyId);
                if (existingDependency != null)
                {
                    db.Dependency.Remove(existingDependency); 
                    db.SaveChanges();
                }
            }
        }
    }


}

