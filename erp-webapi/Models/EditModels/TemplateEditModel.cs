using System;
using System.Collections.Generic;

namespace flexli_erp_webapi.EditModels
{
    public class TemplateEditModel
    {
        public string TemplateId { get; set; }
        public string Description { get; set; }
        
        public List<TemplateEditModel> ChildTemplates { get; set; }
        
        public List<TemplateEditModel> ParentTemplates { get; set; }
        
        public TemplateEditModel CloneTemplate  { get; set; }
        
        public string CloneTemplateId  { get; set; }
        
        public string Owner { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? EditedAt { get; set; }
        
        public string Role { get; set; }
    }
}