using System;

namespace flexli_erp_webapi.EditModels
{
    public enum CStatus { NotCompleted, Completed };
    public enum CResultType { File, Boolean, Numeric };
    public class CheckListItemEditModel
    {
        public string CheckListItemId { get; set; }
        
        public string TaskId { get; set; }
        
        public string Description { get; set; }
        
        public CStatus Status { get; set; }

        public int? WorstCase { get; set; }
        
        public int? BestCase { get; set; }
        
        public CResultType ResultType { get; set; }
        
        public string Result { get; set; }
        
        public string UserComment { get; set; }
        
        public string ManagerComment { get; set; }
        
        public bool Essential { get; set; }
        
    }
}