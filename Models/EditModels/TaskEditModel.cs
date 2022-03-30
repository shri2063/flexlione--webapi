using System;
using System.Collections.Generic;

namespace m_sort_server.EditModels
{
   
    public enum EStatus { yetToStart, ongoing, completed,onHold,dropped };
    public class TaskEditModel
    {
        public string TaskId { get; set; }
        
        public string ParentTaskId { get; set; }
        
        public string CreatedBy { get; set; }
        
        public string AssignedTo { get; set; }
        
        public string PositionAfter { get; set; }
        
        public string Description { get; set; }
        
        public DateTime  CreatedAt { get; set; }
        
        public DateTime?  Deadline { get; set; }
        
        public EStatus Status { get; set; }
        
        public int? Score { get; set; }
        
        public int? Rank { get; set; }
        
        public List<TaskEditModel> Children { get; set; }
        
        public List<DependencyEditModel> DownStreamDependencies { get; set; }
        
        public List<DependencyEditModel> UpStreamDependencies { get; set; }
        
        public List<TaskEditModel> Siblings { get; set; }
    }
}