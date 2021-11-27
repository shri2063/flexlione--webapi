using System;

namespace m_sort_server.Models
{
    public class BotPosition
    {
        public string BotId { get; set; }
        
        public string StationId { get; set; }
        
        public string NodeId { get; set; }
        
        public DateTime Time { get; set; }
        
        public decimal CalAvgSpeed { get; set; }
        
        public string Command { get; set; }
        
        public string ActualSpeed { get; set; }
    }
}