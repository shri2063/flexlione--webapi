using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace flexli_erp_webapi.DataModels
{
    [Table("template")]
    public class Template
    {
        [Key] [Column("template_id")] 
        public string TemplateId { get; set; }

        [Column("description")] 
        public string Description { get; set; }
        
        [Column("owner")] 
        public string Owner { get; set; }

        [Column("clone")] 
        public string CloneTemplateId { get; set; }
        
        [Column("created_at")] 
        public DateTime CreatedAt { get; set; }
        
        [Column("edited_at")] 
        public DateTime? EditedAt { get; set; }
        
        [Column("role")] 
        public string Role { get; set; }
        
      
    }
}