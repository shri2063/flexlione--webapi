using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace flexli_erp_webapi.DataModels
{
    [Table("task_hierarchy")]
    public class TaskHierarchy
    {
        [Key][Column("task_hierarchy_id")]
        public string TaskHierarchyId { get; set; }
        
        [Column("task_id")]
        public string TaskId { get; set; }
        
        [Column("hierarchy")]
        public List<string> TaskIds { get; set; }
        
        public TaskDetail TaskDetail { get; set; }
    }
}