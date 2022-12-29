using System;
using System.Collections.Generic;
using System.Linq;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Repository.Interfaces;
using mflexli_erp_webapi.Repository.Interfaces;

namespace flexli_erp_webapi.Repository
{
    public class DependencyRepository: IDependencyRepository
    {

        private readonly ITaskRepository _taskRepository;
        public DependencyRepository(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }
        public  List<DependencyEditModel> GetUpstreamDependenciesByTaskId(string taskId, int? pageIndex = null, int? pageSize = null)
        {
            List<DependencyEditModel> upStreamDependencies;
           
            using (var db = new ErpContext())
            {

                if (pageIndex != null && pageSize != null)
                {
                    upStreamDependencies = GetUpstreamDependenciesPageForTaskId(taskId, (int) pageIndex, (int) pageSize);
                }

                else
                {
                    upStreamDependencies = db.Dependency
                        .Where(x => x.DependentTaskId == taskId)
                        .Select(s => new DependencyEditModel()
                        {
                            DependencyId = s.DependencyId,
                            DependentTaskId = s.DependentTaskId,
                            Description = s.Description,
                            TaskId = s.TaskId
                        }).ToList();
                }
            }

           
            
            return  upStreamDependencies;
        }

        private static List<DependencyEditModel> GetUpstreamDependenciesPageForTaskId(string taskId, int pageIndex, int pageSize)
        {
            using (var db = new ErpContext())
            {
                if (pageIndex <= 0 || pageSize <= 0)
                    throw new ArgumentException("Incorrect value for pageIndex or pageSize");
                
                // skip take logic
                var upStreamDependencies = db.Dependency
                    .Where(x => x.DependentTaskId == taskId)
                    .Select(s => new DependencyEditModel()
                    {
                        DependencyId = s.DependencyId,
                        DependentTaskId = s.DependentTaskId,
                        Description = s.Description,
                        TaskId = s.TaskId
                    })
                    .OrderByDescending(x=>Convert.ToInt32(x.DependencyId))
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                if (upStreamDependencies.Count == 0)
                {
                    throw new ArgumentException("Incorrect value for pageIndex or pageSize");
                }

                return upStreamDependencies;

            }
        }
        
        public  List<DependencyEditModel> GetDownstreamDependenciesByTaskId(string taskId, int? pageIndex = null, int? pageSize = null)
        {
            List<DependencyEditModel> downStreamDependencies;
            using (var db = new ErpContext())
            {
                if (pageIndex != null && pageSize != null)
                {
                    downStreamDependencies = GetDownstreamDependenciesPageForTaskId(taskId, (int) pageIndex, (int) pageSize);
                }

                else
                {
                    downStreamDependencies = db.Dependency
                        .Where(x => x.TaskId == taskId)
                        .Select(s => new DependencyEditModel()
                        {
                            DependencyId = s.DependencyId,
                            DependentTaskId = s.DependentTaskId,
                            Description = s.Description,
                            TaskId = s.TaskId
                        }).ToList();
                }
                
            }
           

            
            return downStreamDependencies;
        }

        private static List<DependencyEditModel> GetDownstreamDependenciesPageForTaskId(string taskId, int pageIndex, int pageSize)
        {
            using (var db = new ErpContext())
            {
                if (pageIndex <= 0 || pageSize <= 0)
                    throw new ArgumentException("Incorrect value for pageIndex or pageSize");
                
                // skip take logic
                var downStreamDependencies = db.Dependency
                    .Where(x => x.TaskId == taskId)
                    .Select(s => new DependencyEditModel()
                    {
                        DependencyId = s.DependencyId,
                        DependentTaskId = s.DependentTaskId,
                        Description = s.Description,
                        TaskId = s.TaskId
                    })
                    .OrderByDescending(x=>Convert.ToInt32(x.DependencyId))
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                if (downStreamDependencies.Count == 0)
                {
                    throw new ArgumentException("Incorrect value for pageIndex or pageSize");
                }
                    
                return downStreamDependencies;

            }
        }


    }
}