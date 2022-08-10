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
        
        [Column("child_template_ids")] 
        public List<string> ChildTemplateIds { get; set; }
        
        [Column("clone")] 
        public string CloneTemplateId { get; set; }
        
        [Column("created_at")] 
        public DateTime CreatedAt { get; set; }
    }
}