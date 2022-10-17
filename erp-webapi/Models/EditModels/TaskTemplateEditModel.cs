using System.Collections.Generic;

namespace flexli_erp_webapi.EditModels
{
    public class TaskTemplateEditModel
    {

        public string TemplateId { get; set; }
        
        public Dictionary<string, string> RoleProfileMap { get; set; }
        
        public string ParentTaskId { get; set; }
        
        public bool IncludeReference { get; set; }
        
        public bool IncludeAllChildren { get; set; }
        
        public string CreatedBy { get; set; }
    }
}