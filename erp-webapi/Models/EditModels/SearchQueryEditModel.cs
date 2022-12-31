using System;
using System.Collections.Generic;

namespace flexli_erp_webapi.EditModels
{
    public class SearchQueryEditModel
    
    
    {
        public DateTime?  Deadline { get; set; }
        
        public List<string> CreatedBy { get; set; }
        
        public List<string> AssignedTo { get; set; }
        
        public string Description { get; set; }
        
        public string TaskId { get; set; }
        
        public bool IncludeRemoved { get; set; }
        
        public List<string> Status { get; set; }
    }
}