using System;
using System.Collections.Generic;

namespace m_sort_server.EditModels
{
    public class SearchQueryEditModel
    
    
    {
        public string Tag { get; set; }
        public DateTime?  Deadline { get; set; }
        
        public List<string> CreatedBy { get; set; }
        
        public List<string> AssignedTo { get; set; }
        
        public string Description { get; set; }
        
        public List<string> Status { get; set; }
    }
}