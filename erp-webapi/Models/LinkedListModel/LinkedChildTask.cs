using flexli_erp_webapi.EditModels;

namespace flexli_erp_webapi.LinkedListModel
{
    public class LinkedChildTask
    {
        public  TaskDetailEditModel TaskDetail { get; set; }
        
       
        public  LinkedChildTask Next { get; set; }
        
        public  LinkedChildTask Previous { get; set; }
    }
}