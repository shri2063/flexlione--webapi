using System;
using System.Collections.Generic;

namespace m_sort_server.Models
{
    public class Bot
    {
        public string BotId { get; set; }
        
        public BotPosition BotPosition { get; set; }
        
        public BotConfig BotConfig { get; set; }
        
       
        
        public  BotMotor BotMotor { get; set; }
        
        public Node Node { get; set; }
        
        public  Station Station { get; set; }
    }

    public class Node
    {
        public string NodeId { get; set; }
        
        public DateTime Time { get; set; }
        
        public decimal Pointer { get; set; }
        
        public Node LastNode { get; set; }
    }
    
    public class Station
    {
        public string StationId { get; set; }
        
        public DateTime Time { get; set; }

        public Station LastStation { get; set; }
    }
}