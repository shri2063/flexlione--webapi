namespace flexli_erp_webapi.EditModels
{
    public enum EUnplannedTaskStatus { notRequested, requested, reviewed };
    public class SprintUnplannedTaskDataEditModel
    {
        public string Id { get; set; }
        
        public string SprintId { get; set; }
        
        public string TaskId { get; set; }
        
        public EUnplannedTaskStatus ScoreStatus { get; set; }
        
        public string ProfileId { get; set; }
        
        public int? RequestedHours { get; set; }
        
        public int? ApprovedHours { get; set; }
    }
}