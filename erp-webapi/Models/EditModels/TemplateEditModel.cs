using System.Collections.Generic;

namespace m_sort_server.EditModels
{
    public class TemplateEditModel
    {
        public string TemplateId { get; set; }

        public string Description { get; set; }
        
        public List<TaskDetailEditModel> TaskList { get; set; }
    }
}