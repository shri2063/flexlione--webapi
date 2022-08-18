using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace flexli_erp_webapi.DataModels
{
    [Table("sprint_report")]
    public class SprintReport
    {
        [Key][Column("sprint_report_line_item_id")]
        public string SprintReportLineItemId { get; set; }
        
        [Column("sprint_id")]
        public string SprintId { get; set; }
        
        [Column("task_id")]
        public string TaskId { get; set; }
        
        [Column("checklist_item_id")]
        public string CheckListItemId { get; set; }
        
        [Column("description")]
        public string Description { get; set; }
        
        [Column("result_type")]
        public string ResultType { get; set; }
        
        [Column("result")]
        public string Result { get; set; }
        
        [Column("user_comment")]
        public string UserComment { get; set; }
        
        [Column("manager_comment")]
        public string ManagerComment { get; set; }
        
        [Column("approved")]
        public string Approved { get; set; }
        
        [Column("status")]
        public string Status { get; set; }
        
        [Column("worst_case")]
        public int? WorstCase { get; set; }
        
        [Column("best_case")]
        public int? BestCase { get; set; }
        
        [Column("score")]
        public int? Score { get; set; }
        
        [Column("essential")]
        public bool Essential { get; set; }
    }
}