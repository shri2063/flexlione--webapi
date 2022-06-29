using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace flexli_erp_webapi.DataModels
{
    [Table("template_task")]
    public class TemplateTask
    {
        [Key] [Column("template_task_id")] 
        public string TemplateTaskId { get; set; }

        [Column("template_id")] 
        public string TemplateId { get; set; }
        
        [Column("task_id")] 
        public string TaskId { get; set; }
    }
}