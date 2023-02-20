using System;

namespace flexli_erp_webapi.EditModels
{
    public class TaskShortDetailEditModel
    {
        public string TaskId { get; set; }
        
        public string Description { get; set; }
        
        public EStatus Status { get; set; }
        
        public DateTime? Deadline { get; set; }
    }
}