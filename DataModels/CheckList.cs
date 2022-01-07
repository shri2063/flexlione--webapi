using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace m_sort_server.DataModels
{
   
    [Table("task_sheet")]
    public class TaskSheet
    {
       [Key][Column("task_id")]
        public string TaskId { get; set; }
        
        [Column("parent_task_id")]
        public string ParentTaskId { get; set; }
        
        [Column("created_by")]
        public string CreatedBy { get; set; }
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
        
        [Column("on_hold")]
        public bool OnHold { get; set; }
        
        [Column("description")]
        public string Description { get; set; }
        
        [Column("position_after")]
        public string PositionAfter { get; set; }

        
       
        
       
    }
}