namespace m_sort_server.EditModels
{
    public class DependencyEditModel
    {
       
        
            public string DependencyId { get; set; }
        
          
            public string TaskId { get; set; }
            
            
            public string DependentTaskId { get; set; }
            
           
            public string Description { get; set; }
            
            public  TaskEditModel TaskEditModel { get; set; }
        
    }
}