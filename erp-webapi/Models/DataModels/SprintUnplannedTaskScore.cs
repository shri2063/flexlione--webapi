using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace flexli_erp_webapi.DataModels
{
    [Table("sprint_unplanned_tasks")]
    public class SprintUnplannedTaskScore
    {
        [Key][Column("id")]
        public string Id { get; set; }
        
        [Column("sprint_id")]
        public string SprintId { get; set; }
        
        [Column("task_id")]
        public string TaskId { get; set; }
        
        [Column("requested_hrs")]
        public int RequestedHours { get; set; }
        
        [Column("approved_hrs")]
        public int ApprovedHours { get; set; }
        
        [Column("status")]
        public string ScoreStatus { get; set; }
        
        [Column("profile_id")]
        public string ProfileId { get; set; }
    }
}