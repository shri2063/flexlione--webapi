using System;
using System.Threading.Tasks;
using flexli_erp_webapi;
using flexli_erp_webapi.Controller;
using flexli_erp_webapi.EditModels;
using m_sort_server;
using Xunit;
using flexli_erp_webapi.Controller;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Repository.Interfaces;
using flexli_erp_webapi.Services;


[assembly: CollectionBehavior(CollectionBehavior.CollectionPerClass, DisableTestParallelization = true)]

namespace api_unit_testing
{
    [Collection("Sequential")]
    public class TaskControllerTest
    {
        private readonly TaskController _taskController;

        private readonly TaskDetailEditModel _taskDetailEditModel = new TaskDetailEditModel();
        
     
        
        private readonly TaskManagementService _taskManagementService;
       
        

        public TaskControllerTest(
            TaskManagementService taskManagementService)
        {
          
            _taskManagementService = taskManagementService;
          //  _taskController = new TaskController( _taskManagementService);
            string connString = "Server=65.1.53.71;Port=5432;UserId=postgres;Password=3edc#EDC;Database=flexli-erp-alpha;";
            ErpContext.SetConnectionString(connString);
            
        }


        [Fact]
        public void ReadTask()
        {
            TaskDetailEditModel taskDetailEditModel =  _taskController.GetTaskById("23","children").Value;
                Assert.Equal("23", taskDetailEditModel.TaskId);
        }
    }    
}
