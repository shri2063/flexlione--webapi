namespace flexli_erp_webapi.EditModels
{
    public class TaskShortScheduleEditModel
    {
        public string TaskScheduleId { get; set; }
        
        public bool IsPlanned { get; set; }
        
        public int StartHour { get; set; }
    }
}