using System;

namespace m_sort_server.EditModels
{
    public class CheckListItemEditModel
    {
        public string CheckListId { get; set; }
        
        public string TaskId { get; set; }
        
        public string Description { get; set; }
        
        public string Status { get; set; }
        
        public string Comment { get; set; }
        
        public string  Attachment { get; set; }
        
    }
}