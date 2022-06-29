namespace flexli_erp_webapi.EditModels
{
    public class DependencyEditModel
    {
       
        
            public string DependencyId { get; set; }
        
          
            public string TaskId { get; set; }
            
            
            public string DependentTaskId { get; set; }
            
           
            public string Description { get; set; }
            
            public  TaskDetailEditModel TaskDetailEditModel { get; set; }
        
    }
}