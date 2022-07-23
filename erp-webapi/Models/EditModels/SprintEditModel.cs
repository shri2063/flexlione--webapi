using System;
using System.Collections.Generic;

namespace flexli_erp_webapi.EditModels
{
    public enum SStatus { planning, requestforapproval, approved, requestforclosure, closed, reviewed };
    public class SprintEditModel
    {
        public string SprintId { get; set; }

        public string Description { get; set; }
        
        public string Owner { get; set; }
        
        public DateTime FromDate { get; set; }
        
        public DateTime ToDate { get; set; }
        
        public int Score { get; set; }
        
        public SStatus Status { get; set; }
        
        public double SprintNo { get; set; }

        public bool Approved { get; set; }
        
        public bool Closed { get; set; }
        
        public List<TaskDetailEditModel> Tasks { get; set; }
        
    }
}