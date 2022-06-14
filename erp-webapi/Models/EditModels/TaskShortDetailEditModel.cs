namespace m_sort_server.EditModels
{
    public class TaskShortDetailEditModel
    {
        public string TaskId { get; set; }
        
        public string Description { get; set; }
        
        public EStatus Status { get; set; }
    }
}