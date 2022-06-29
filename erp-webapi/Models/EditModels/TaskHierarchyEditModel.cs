using System.Collections.Generic;

namespace flexli_erp_webapi.EditModels
{
    public class TaskHierarchyEditModel
    {
        public string TaskHierarchyId { get; set; }
        public string TaskId { get; set; }
        
        public string Description { get; set; }
        
        public decimal TotalEstimatedHours { get; set; }
        public decimal TotalHoursSpent { get; set; }
      
        public List<string> ChildrenTaskIdList { get; set; }
        
        public List<TaskHierarchyEditModel> ChildrenTaskHierarchy { get; set; }
    }
}