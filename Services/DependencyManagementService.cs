using System;
using System.Collections.Generic;
using System.Linq;
using m_sort_server.DataModels;
using m_sort_server.EditModels;

namespace m_sort_server.Services
{
    public class DependencyManagementService
    {
        public static List<DependencyEditModel> GetUpstreamDependenciesByTaskId(string taskId,string include = null)
        {

            List<DependencyEditModel> upStreamDependencies;
           
            using (var db = new ErpContext())
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

            if (include == null)
            {
                return upStreamDependencies;
            }

            if (include.ToLower() == "taskDetail")
            {
               
                upStreamDependencies.ForEach(x =>
                    x.TaskDetailEditModel = TaskManagementService.GetTaskById(x.DependentTaskId));
            }

            
            return upStreamDependencies;
        }
        
        public static List<DependencyEditModel> GetDownstreamDependenciesByTaskId(string taskId,string include = null)
        {
            
            List<DependencyEditModel> downStreamDependencies;
            using (var db = new ErpContext())
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
            if (include == null)
            {
                return downStreamDependencies;
            }

            if (include.ToLower() == "taskDetail")
            {
                downStreamDependencies.ForEach(x =>
                    x.TaskDetailEditModel = TaskManagementService.GetTaskById(x.DependentTaskId));
            }

            
            return downStreamDependencies;
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

