using System.Collections.Generic;

namespace m_sort_server.Models
{
    
    
    public class NodeJsonModel 
    {
       
        public string NodeId { get; set; }
        
        public string GraphId { get; set; }

        public string Index  { get; set; }

        public bool Junction { get; set; } = false;

        public bool Source { get; set; } = false;
        
        public bool DropOff { get; set; } = false;

        public bool Exit { get; set; } = false;

        public List<KeyValuePair<string, decimal>> Edges { get; set; }
        
        public List<string> ForwardNodes { get; set; }
        
        public Coordinate Coordinates { get; set; }
        
       
    }
}