using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace m_sort_server.DataModels
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