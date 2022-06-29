using System;
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
    }
}