using System;
using System.Collections.Generic;
using m_sort_server;
using m_sort_server.Controller;
using m_sort_server.EditModels;
using m_sort_server.Services;
using Xunit;

namespace api_unit_testing
{
    [Collection("Sequential")]
    
    public class CheckListItemControllerTest
    {
        public CheckListItemControllerTest()
        {
            string connString = "Server=65.1.53.71;Port=5432;UserId=postgres;Password=3edc#EDC;Database=flexli-erp-alpha";
            ErpContext.SetConnectionString(connString);
        }

        private const string DESCRIPTION = "unit-testing-description";
        private const string STATUS = "unit-testing-status";
        private const string COMMENT = "unit-testing-comment";
        private const string ATTACHEMENT = "unit-testing-attachment";

        private static CheckListItemEditModel _checkListItem = new CheckListItemEditModel()
        {
            CheckListItemId = "",
            TaskId = "8",
            Description = DESCRIPTION,
            Status = STATUS,
            Comment = COMMENT,
            Attachment = ATTACHEMENT
        };
        
        private static CheckListItemEditModel _updateCheckListItem = new CheckListItemEditModel()
        {
            CheckListItemId = "33",
            TaskId = "8",
            Description = DESCRIPTION,
            Status = STATUS,
            Comment = COMMENT,
            Attachment = ATTACHEMENT
        };

        [Fact]
        private void givenInvalidTaskId_whenGetCheckListCalled_expectedNull()
        {
            Assert.Empty(CheckListManagementService.GetCheckList("non-existent-taskId", "items"));
        }

        [Fact]
        private void givenExistingTaskIdAndIncludeContainItems_whenGetCheckListCalled_expectedNotNull()
        {
            //CheckListManagementService.CreateOrUpdateCheckListItem(_checkListItem);
            List<CheckListItemEditModel> checkList = CheckListManagementService.GetCheckList("8", "items");
            Assert.NotEmpty(checkList);
        }

        [Fact]
        private void givenExistingTaskIdAndIncludeNotContainsItems_whenGetCheckListCalled_expectedException()
        {
            Assert.Throws<KeyNotFoundException>(() => CheckListManagementService.GetCheckList("8", "any-type"));
        }

        [Fact]
        private void whenCreatedCheckList_expectCorrectCreation()
        {
            CheckListItemEditModel checkListItem =
                CheckListManagementService.CreateOrUpdateCheckListItem(_checkListItem);
            Assert.NotNull(checkListItem);
            
            int flag = 0;
            List<CheckListItemEditModel> itemList = CheckListManagementService.GetCheckList("8", "items");
            itemList.ForEach(x =>
            {
                if (x.CheckListItemId == checkListItem.CheckListItemId)
                {
                    flag = 1;
                }
                    
            });
            
            Assert.Equal(1, flag);

            // List<CheckListItemEditModel> itemList = CheckListManagementService.GetCheckList("8", "items");
            // Assert.Contains(checkListItem, itemList);
        }

        [Fact]
        private void forExistingCheckListItem_whenUpdated_expectNoError()
        {
            var temp = DateTime.Now.ToString("T");
            _updateCheckListItem.Description = DESCRIPTION + temp;
            CheckListItemEditModel updatedCheckListItem = CheckListManagementService.CreateOrUpdateCheckListItem(_updateCheckListItem);
            Assert.NotNull(updatedCheckListItem);

            int flag = 0;
            List<CheckListItemEditModel> itemList = CheckListManagementService.GetCheckList("8", "items");
            itemList.ForEach(x =>
            {
                if (x.Description == _updateCheckListItem.Description)
                {
                    flag = 1;
                }
                    
            });
            
            Assert.Equal(1, flag);
        }

        [Fact]
        private void whenDeleteCheckListItemCalled_expectSuccessAndNoException()
        {
            CheckListManagementService.DeleteCheckListItem("non-existent-checklist-id");
            CheckListItemEditModel checkListItem =
                CheckListManagementService.CreateOrUpdateCheckListItem(_checkListItem);
            CheckListManagementService.DeleteCheckListItem(checkListItem.CheckListItemId);
            
            List<CheckListItemEditModel> itemList = CheckListManagementService.GetCheckList("8", "items");
            itemList.ForEach(x=>{Assert.NotEqual(x.CheckListItemId, checkListItem.CheckListItemId);});
            
        }
    }
}