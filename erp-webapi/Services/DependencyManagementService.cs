using System;
using System.Collections.Generic;
using System.Linq;
using flexli_erp_webapi.DataModels;
using flexli_erp_webapi.EditModels;

namespace flexli_erp_webapi.Services
{
    public class DependencyManagementService
    {
        public static List<DependencyEditModel> GetUpstreamDependenciesByTaskId(string taskId,string include = null, int? pageIndex = null, int? pageSize = null)
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

            if (include == null)
            {
                return upStreamDependencies;
            }

            if (include.Contains( "task"))
            {
               
                upStreamDependencies.ForEach(x =>
                    x.TaskDetailEditModel = TaskManagementService.GetTaskById(x.DependentTaskId));
            }
            
            return upStreamDependencies;
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

        public static List<DependencyEditModel> GetDownstreamDependenciesByTaskId(string taskId,string include = null, int? pageIndex = null, int? pageSize = null)
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
            if (include == null)
            {
                return downStreamDependencies;
            }

            if (include.Contains( "task"))
            {
                downStreamDependencies.ForEach(x =>
                    x.TaskDetailEditModel = TaskManagementService.GetTaskById(x.DependentTaskId));
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

        private static DependencyEditModel GetDependencyById(string dependencyId)
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
        
        public static DependencyEditModel CreateOrUpdateDependency(DependencyEditModel dependencyEditModel)
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
        
        private static string GetNextAvailableDependencyId()
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
        
        public static void DeleteDependency(string dependencyId)
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

