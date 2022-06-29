using System;

namespace flexli_erp_webapi.EditModels
{
    public class CheckListItemEditModel
    {
        public string CheckListItemId { get; set; }
        
        public string TaskId { get; set; }
        
        public string Description { get; set; }
        
        public string Status { get; set; }
        
        public string Comment { get; set; }
        
        public string  Attachment { get; set; }
        
    }
}