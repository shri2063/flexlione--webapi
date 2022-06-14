using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace m_sort_server.DataModels
{
   
        [Table("dependency")]
        public class Dependency
        {
            [Key] [Column("dependency_id")]
            public string DependencyId { get; set; }
        
            [Column("task_id")]
            public string TaskId { get; set; }
            
            [Column("dependent_task_id")]
            public string DependentTaskId { get; set; }
            
            [Column("description")]
            public string Description { get; set; }
        }

    
}