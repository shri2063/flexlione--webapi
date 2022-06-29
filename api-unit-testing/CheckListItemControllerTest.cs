using System;
using System.Collections.Generic;
using System.Linq;
using m_sort_server;
using m_sort_server.Controller;
using m_sort_server.EditModels;
using m_sort_server.Services;
using m_sort_server.Utility;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace api_unit_testing
{
    [Collection("Sequential")]
    
    public class CheckListItemControllerTest : IDisposable
    {
        //Runs before every test
        public CheckListItemControllerTest()
        {
            string connString = "Server=65.1.53.71;Port=5432;UserId=postgres;Password=3edc#EDC;Database=flexli-erp-alpha";
            ErpContext.SetConnectionString(connString);

            TaskDetailEditModel _createdTask = TaskManagementService.CreateOrUpdateTask(_taskDetailEditModel);
            TASKID = _createdTask.TaskId;
            
            TaskHierarchyManagementService.DeleteTaskHierarchy(_createdTask.TaskId);
        }
        
        //Runs after every test
        public void Dispose()
        {
            using (var db = new ErpContext())
            {
                db.CheckList.RemoveIfExists(CHECKLISTITEMID);
                db.SaveChanges();

                db.TaskDetail.RemoveIfExists(TASKID);
                db.SaveChanges();
            }
        }

        private readonly TaskDetailEditModel _taskDetailEditModel = new TaskDetailEditModel()
        {
            TaskId = "new-task",
            ParentTaskId = "0",
            CreatedBy = "14",
            AssignedTo = "14",
            Description = DESCRIPTION,
            PositionAfter = "577"
        
        };

        private static string CHECKLISTITEMID = "unit-test-123";
        private static string TASKID = "task-id-123";
        private const string DESCRIPTION = "unit-testing-description";
        private const string STATUS = "unit-testing-status";
        private const string COMMENT = "unit-testing-comment";
        private const string ATTACHEMENT = "unit-testing-attachment";

        private const string NONEXISTANTTASKID = "non-existent-taskId";
        private const string ITEMS = "items";
        private const string NONITEMS = "any-string";

        private const string NONITEMMESSAGE = "Error in finding required check list";
        private const string INVALIDTASKIDMESSAGE =
            "An error occurred while updating the entries. See the inner exception for details.";

        private static CheckListItemEditModel _checkListItem = new CheckListItemEditModel()
        {
            CheckListItemId = CHECKLISTITEMID,
            Description = DESCRIPTION,
            Status = STATUS,
            Comment = COMMENT,
            Attachment = ATTACHEMENT
        };
        
        private readonly CheckListItemEditModel _invalidCheckListItem = new CheckListItemEditModel()
        {
            CheckListItemId = CHECKLISTITEMID,
            TaskId = NONEXISTANTTASKID,
            Description = DESCRIPTION,
            Status = STATUS,
            Comment = COMMENT,
            Attachment = ATTACHEMENT
        };

        [Fact]
        private void givenInvalidTaskId_whenGetCheckListCalled_expectedNull() //read
        {
            // Arrange

            // Act
            List<CheckListItemEditModel> checkList =
                CheckListManagementService.GetCheckList(NONEXISTANTTASKID, ITEMS);
            
            // Assert
            Assert.Empty(checkList);
        }

        [Fact]
        private void givenExistingTaskIdAndIncludeContainItems_whenGetCheckListCalled_expectedNotNull() //read
        {
            // Arrange
            _checkListItem.TaskId = TASKID; 
            CheckListItemEditModel checkListItem = CheckListManagementService.CreateOrUpdateCheckListItem(_checkListItem);
            CHECKLISTITEMID = checkListItem.CheckListItemId;

            // Act
            List<CheckListItemEditModel> checkList = CheckListManagementService.GetCheckList(TASKID, ITEMS);
        
            // Assert
            Assert.NotEmpty(checkList);
        }
        
        [Fact]
        private void givenExistingTaskIdAndIncludeNotContainsItems_whenGetCheckListCalled_expectedException() //read
        {
            // Arrange
            _checkListItem.TaskId = TASKID;
            CheckListItemEditModel checkListItem = CheckListManagementService.CreateOrUpdateCheckListItem(_checkListItem);
            CHECKLISTITEMID = checkListItem.CheckListItemId;
            
            // Act
            
            //Assert
            var ex = Assert.Throws<KeyNotFoundException>(() => CheckListManagementService.GetCheckList(TASKID, NONITEMS));
            Assert.Equal(NONITEMMESSAGE, ex.Message);
        }
        
        [Fact]
        private void whenCreatedOrUpdatedCheckList_expectCorrectCreationOrUpdation() //write
        {
            // Arrange
        
            // Act
            _checkListItem.TaskId = TASKID;
            CheckListItemEditModel checkListItem =
                CheckListManagementService.CreateOrUpdateCheckListItem(_checkListItem);
            CHECKLISTITEMID = checkListItem.CheckListItemId;

            CheckListItemEditModel updateEditModel = new CheckListItemEditModel()
            {
                CheckListItemId = CHECKLISTITEMID,
                TaskId = TASKID,
                Description = DESCRIPTION,
                Status = STATUS,
                Comment = COMMENT,
                Attachment = ATTACHEMENT
            };
            var temp = DateTime.Now.ToString("T");
            updateEditModel.Description = DESCRIPTION + temp;
            
            CheckListItemEditModel updatedCheckListItem = CheckListManagementService.CreateOrUpdateCheckListItem(updateEditModel);
            
            // Assert
            Assert.Equal(CHECKLISTITEMID, checkListItem.CheckListItemId);

            int flag = 0;
            List<CheckListItemEditModel> itemList = CheckListManagementService.GetCheckList(TASKID, ITEMS);
            itemList.ForEach(x =>
            {
                if (x.CheckListItemId == CHECKLISTITEMID)
                {
                    flag = 1;
                    Assert.Equal(TASKID, x.TaskId);
                    Assert.Equal(updateEditModel.Description,x.Description);
                    Assert.Equal(STATUS, x.Status);
                    Assert.Equal(ATTACHEMENT, x.Attachment);
                    Assert.Equal(COMMENT, x.Comment);
                }
                    
            });
            Assert.Equal(1,flag);
        }
        
        [Fact]
        private void whenDeleteCheckListItemCalled_expectSuccessAndNoException() //Delete
        {
            // Arrange
            _checkListItem.TaskId = TASKID;
            CheckListItemEditModel checkListItem =
                CheckListManagementService.CreateOrUpdateCheckListItem(_checkListItem);
            CHECKLISTITEMID = checkListItem.CheckListItemId;
            
            // Act
            CheckListManagementService.DeleteCheckListItem("non-existent-checklist-id");
            CheckListManagementService.DeleteCheckListItem(checkListItem.CheckListItemId);
            
            List<CheckListItemEditModel> itemList = CheckListManagementService.GetCheckList(TASKID, ITEMS);
            
            // Assert
            itemList.ForEach(x=>{Assert.NotEqual(CHECKLISTITEMID,x.CheckListItemId);});
            
        }
        
        [Fact]
        private void whenCreatedCheckListWithInvalidTaskId_expectedException() //Relation
        {
            // Arrange
            
            // Act
            
            //Assert
            var ex = Assert.Throws<DbUpdateException>(() =>
                CheckListManagementService.CreateOrUpdateCheckListItem(_invalidCheckListItem));
            Assert.Equal(INVALIDTASKIDMESSAGE, ex.Message);
        }
    }
}