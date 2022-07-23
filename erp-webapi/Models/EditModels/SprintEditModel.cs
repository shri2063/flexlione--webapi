using System;
using System.Collections.Generic;

namespace flexli_erp_webapi.EditModels
{
  
    public class SprintEditModel
    {
        public string SprintId { get; set; }

        public string Description { get; set; }
        
        public string Owner { get; set; }
        
        public DateTime FromDate { get; set; }
        
        public DateTime ToDate { get; set; }
        
        public int Score { get; set; }
        
        public string Deliverable { get; set; }
        
        public string Delivered { get; set; }
        
        public List<TaskDetailEditModel> Tasks { get; set; }
        
    }
}