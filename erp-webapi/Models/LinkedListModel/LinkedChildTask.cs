using m_sort_server.EditModels;

namespace m_sort_server.LinkedListModel
{
    public class LinkedChildTask
    {
        public  TaskDetailEditModel TaskDetail { get; set; }
        
       
        public  LinkedChildTask Next { get; set; }
        
        public  LinkedChildTask Previous { get; set; }
    }
}