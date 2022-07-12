﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace flexli_erp_webapi.DataModels
{
   
    [Table("task_detail")]
    public class TaskDetail
    {
       [Key][Column("task_id")]
        public string TaskId { get; set; }
        
        [Column("parent_task_id")]
        public string ParentTaskId { get; set; }
        
        [Column("created_by")]
        public string CreatedBy { get; set; }
        
        [Column("assigned_to")]
        public string AssignedTo { get; set; }
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
        
        [Column("deadline")]
        public DateTime? Deadline { get; set; }
        
        [Column("status")]
        public string Status { get; set; }
        
        [Column("description")]
        public string Description { get; set; }
        
        [Column("sprint_id")]
        public string SprintId { get; set; }
        
        [Column("position_after")]
        public string PositionAfter { get; set; }

        [Column("rank")]
        public int? Rank { get; set; }
        
        [Column("is_removed")]
        public bool? IsRemoved { get; set; }
        
        [Column("expected_hours")]
        public decimal? ExpectedHours { get; set; }
        
        [Column("actual_score")]
        public int? ActualScore { get; set; }
        
        [Column("best_score")]
        public int? BestScore { get; set; }
        
        [Column("worst_score")]
        public int? WorstScore { get; set; }
        
        [Column("expected_deliverable")]
        public string ExpectedDeliverable { get; set; }
        
        [Column("Delivered")]
        public string Delivered { get; set; }
        
        [Column("edited_at")]
        public DateTime EditedAt { get; set; }
        
        public List<TaskSchedule> TaskSchedules { get; set; }

        
       
        
       
    }
}