namespace m_sort_server.EditModels
{
    public class TaskShortScheduleEditModel
    {
        public string TaskScheduleId { get; set; }
        
        public bool IsPlanned { get; set; }
        
        public int StartHour { get; set; }
    }
}