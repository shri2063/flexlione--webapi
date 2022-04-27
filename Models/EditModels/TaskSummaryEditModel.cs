using System;

namespace m_sort_server.EditModels
{
    public class TaskSummaryEditModel
    {
     
        public string TaskSummaryId { get; set; }

       
        public string TaskId { get; set; }

        public string Description { get; set; }
        
     
        public DateTime Date { get; set; }
        
      
        public decimal ExpectedHour { get; set; }
        
   
        public string ExpectedOutput { get; set; }
        
     
        public string ActualOutput { get; set; }
        

        public decimal ActualHour { get; set; }
        
        public string TaskScheduleId { get; set; }
    }
}