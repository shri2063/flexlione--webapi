using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace m_sort_server.DataModels
{
    [Table("task_schedule")]
    public class TaskSchedule
    {
        [Key] [Column("task_schedule_id")] 
        public string TaskScheduleId { get; set; }

        [Column("owner")] 
        public string Owner { get; set; }

        [Column("task_id")]
        public string TaskId { get; set; }
        
        
        [Column("date")]
        public DateTime Date { get; set; }
        
        [Column("start_hour")]
        public int StartHour { get; set; }

        [Column("stop_hour")]
        public int StopHour { get; set; }

        [Column("start_minute")]
        public int StartMinute { get; set; }

        [Column("stop_minute")]
        public int StopMinute { get; set; }
        
        [Column("is_planned")]
        public bool IsPlanned { get; set; }
        
        public TaskSummary TaskSummary { get; set; }
        
        public TaskDetail TaskDetail { get; set; }

    }
}