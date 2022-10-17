using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace flexli_erp_webapi.DataModels
{
    [Table("template_relation")]
    public class TemplateRelation
    {
        [Key] [Column("template_relation_id")] 
        public string TemplateRelationId { get; set; }

        [Column("template_id")] 
        public string TemplateId { get; set; }
        
        [Column("parent_template_id")] 
        public string ParentTemplateId { get; set; }
    }
}