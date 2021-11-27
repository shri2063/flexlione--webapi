using System.Collections.Generic;

namespace m_sort_server.Models
{
    public class EdgeJsonModel
    {
        public string EdgeId { get; set; }
        public string GraphId { get; set; }
        // If BotDataModel is Active or Inactive

        public Coordinate FromCoordinate { get; set; }
        public Coordinate ToCoordinate { get; set; }

        public string EdgeType { get; set; }
        
        public string Direction { get; set; }
        
        public decimal Radius { get; set; }
        
        public bool ClockWise { get; set; }
        public decimal Length { get; set; }
        
        public List<string> Nodes { get; set; }
        
        public List<string> DropOffs { get; set; }
        
        public List<decimal> Distances { get; set; }
        
        public List<string> Sources { get; set; }
        
        public List<string> Exits { get; set; }

        public bool Origin { get; set; } = false;
    }

    public class Coordinate
    {
        public decimal XVertex { get; set; }
        
        public decimal YVertex { get; set; }
    }
}