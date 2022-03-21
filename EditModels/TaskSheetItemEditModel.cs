using System;

namespace m_sort_server.EditModels
{
    public class TaskSheetItemEditModel
    {
        public string TaskId { get; set; }
        
        public string ParentTaskId { get; set; }
        
        public string CreatedBy { get; set; }
        
        public string AssignedTo { get; set; }
        
        public string PositionAfter { get; set; }
        
        public string Description { get; set; }
        
        public DateTime  CreatedAt { get; set; }
        
        public DateTime?  Deadline { get; set; }
        
        public string Status { get; set; }
        
        public int? Score { get; set; }
        
        public int? Rank { get; set; }
    }
}