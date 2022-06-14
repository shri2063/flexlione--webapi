using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace m_sort_server.DataModels
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