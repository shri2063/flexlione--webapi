using m_sort_server.EditModels;

namespace m_sort_server.LinkedListModel
{
    public class LinkedChildTask
    {
        public  TaskEditModel Task { get; set; }
        
       
        public  LinkedChildTask Next { get; set; }
        
        public  LinkedChildTask Previous { get; set; }
    }
}