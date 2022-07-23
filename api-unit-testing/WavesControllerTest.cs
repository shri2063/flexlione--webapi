using System;
using flexli_erp_webapi;
using Xunit;
using flexli_erp_webapi.Controller;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Repository.Interfaces;


[assembly: CollectionBehavior(CollectionBehavior.CollectionPerClass, DisableTestParallelization = true)]

namespace api_unit_testing
{
    [Collection("Sequential")]
    public class TaskControllerTest
    {
        private readonly TaskController _taskController;

        private readonly TaskDetailEditModel _taskDetailEditModel = new TaskDetailEditModel();
        
        private readonly ITagRepository _tagRepository;
        private readonly ITagTaskListRepository _tagTaskListRepository;
       
        

        public TaskControllerTest(ITagRepository tagRepository, ITagTaskListRepository tagTaskListRepository)
        {
            _tagRepository = tagRepository;
            _tagTaskListRepository = tagTaskListRepository;
            _taskController = new TaskController(_tagRepository,_tagTaskListRepository);
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
