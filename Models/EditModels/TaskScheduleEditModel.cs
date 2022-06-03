using System;

namespace m_sort_server.EditModels
{
    public class TaskScheduleEditModel
    {
        
        public string TaskScheduleId { get; set; }

       
        public string Owner { get; set; }

        
        public string TaskId { get; set; }


        public string Description { get; set; }
        
        
        public DateTime Date { get; set; }
        
        
        public int StartHour { get; set; }

        public int StopHour { get; set; }

    
        public int StartMinute { get; set; }

       
        public int StopMinute { get; set; }
        
        public string TaskSummaryId { get; set; }
        
        public TaskShortSummaryEditModel TaskSummary { get; set; }

    }
}