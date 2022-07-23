using System;
using System.Collections.Generic;

namespace flexli_erp_webapi.EditModels
{
   
    public enum EStatus { yettostart, ongoing, completed,onhold,dropped };
    public class TaskDetailEditModel
    {
        public string TaskId { get; set; }
        
        public string ParentTaskId { get; set; }
        
        public string CreatedBy { get; set; }
        
        public string AssignedTo { get; set; }
        
        public string PositionAfter { get; set; }
        
        public string Description { get; set; }
        
        public string SprintId { get; set; }
        
        public decimal? ExpectedHours { get; set; }
        
        public DateTime  CreatedAt { get; set; }
        
        public DateTime?  Deadline { get; set; }
        
        public EStatus Status { get; set; }
        
        public int? Score { get; set; }
        
        public int? AcceptanceCriteria { get; set; }

        public DateTime? EditedAt { get; set; }
        
        public int? Rank { get; set; }

        public bool? IsRemoved { get; set; } = false;
        
        public List<TaskDetailEditModel> Children { get; set; }
        
        public List<DependencyEditModel> DownStreamDependencies { get; set; }
        
        public List<DependencyEditModel> UpStreamDependencies { get; set; }
        
        public List<TaskDetailEditModel> Siblings { get; set; }
    }
}