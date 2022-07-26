namespace flexli_erp_webapi.EditModels
{
    public enum SApproved { NoAction, True,False };
    public class SprintReportEditModel
    {
        public string SprintReportLineItemId { get; set; }
        
        public string SprintId { get; set; }
        
        public string TaskId { get; set; }
        
        public string CheckListItemId { get; set; }
        
        public string Description { get; set; }
        
        public CResultType ResultType { get; set; }
        
        public string Result { get; set; }
        
        public string UserComment { get; set; }
        
        public string ManagerComment { get; set; }
        
        public SApproved Approved { get; set; }
        
        public CStatus Status { get; set; }
        
        public int? WorstCase { get; set; }
        
        public int? BestCase { get; set; }
        
        public int? Score { get; set; }
    }
}