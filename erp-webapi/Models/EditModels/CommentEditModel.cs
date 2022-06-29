using System;

namespace flexli_erp_webapi.EditModels
{
    public class CommentEditModel
    {
        
        public string CommentId { get; set; }
        
        
        public string Message { get; set; }
            
        
        public string TaskId { get; set; }
            
        
        public DateTime CreatedAt { get; set; }
        
        
        public string CreatedBy { get; set; }
    }
}