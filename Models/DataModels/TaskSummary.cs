using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace m_sort_server.DataModels
{
    [Table("task_summary")]
    public class TaskSummary
    
    {
        [Key] [Column("task_summary_id")] 
        public string TaskSummaryId { get; set; }

        [Column("task_id")] 
        public string TaskId { get; set; }

        [Column("date")]
        public DateTime Date { get; set; }
        
        [Column("expected_hour")]
        public decimal ExpectedHour { get; set; }
        
        [Column("expected_output")]
        public string ExpectedOutput { get; set; }
        
        [Column("actual_output")]
        public string ActualOutput { get; set; }
        
        [Column("actual_hour")]
        public decimal ActualHour { get; set; }
        
        [Column("task_schedule_id")]
        public string TaskScheduleId { get; set; }
        
        public TaskDetail TaskDetail { get; set; }
        
    }
}