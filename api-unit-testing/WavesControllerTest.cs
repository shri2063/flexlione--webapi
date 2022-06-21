using System;
using System.Collections.Generic;
using m_sort_server;
using Xunit;
using m_sort_server.Controller;
using m_sort_server.EditModels;
using m_sort_server.Services;


[assembly: CollectionBehavior(CollectionBehavior.CollectionPerClass, DisableTestParallelization = true)]

namespace api_unit_testing
{
    [Collection("Sequential")]
    public class TaskControllerTest
    {
        private readonly TaskController _taskController;

        private readonly TaskDetailEditModel _taskDetailEditModel = new TaskDetailEditModel();

        public TaskControllerTest()
        {
            _taskController = new TaskController();
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
