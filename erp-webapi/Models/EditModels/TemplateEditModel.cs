using System.Collections.Generic;

namespace flexli_erp_webapi.EditModels
{
    public class TemplateEditModel
    {
        public string TemplateId { get; set; }

        public string Description { get; set; }
        
        public List<TaskDetailEditModel> TaskList { get; set; }
    }
}