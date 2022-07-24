using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using flexli_erp_webapi.DataModels;
using flexli_erp_webapi.EditModels;
using Microsoft.AspNetCore.Mvc;


namespace flexli_erp_webapi.Services
{
    public class CheckListManagementService
    {
        public static List<CheckListItemEditModel> GetCheckList(string taskId, string include)
        {
           
            using (var db = new ErpContext())
            {
               
                if (include.Contains("items"))
                {
                    return GetCheckListForATaskId(taskId);
                }

                throw new KeyNotFoundException("Error in finding required check list");
            }
        }
        public static CheckListItemEditModel CreateOrUpdateCheckListItem(CheckListItemEditModel checkListItemEditModel)
        {

            return CreateOrUpdateCheckListInDb(checkListItemEditModel);
            
            
        }

        public static void DeleteCheckListItem(string checkListItemId)
        {
            using (var db = new ErpContext())
            {
                

                // Get Selected TasK
                CheckList existingCheckList = db.CheckList
                    .FirstOrDefault(x => x.CheckListItemId == checkListItemId);
                



                if (existingCheckList != null)
                {
                    var sprintId = db.TaskDetail
                        .Where(x => x.TaskId == existingCheckList.TaskId)
                        .Select(x => x.SprintId)
                        .ToString();

                    if (SprintManagementService.CheckApproved(sprintId))
                    {
                        throw new ConstraintException("Checklist cannot be deleted, sprint is already approved");
                    }
                    
                    db.CheckList.Remove(existingCheckList);
                    db.SaveChanges();
                }


            }
        }

        
      

        
        private static CheckListItemEditModel CreateOrUpdateCheckListInDb(CheckListItemEditModel checkListItemEditModel)
        {
            CheckList checkList;
            bool newCheckList = false;
            // [proxy] Using sprint status as planning  in case sprint not associated to the task
            SStatus status = SStatus.Planning ;
            
            using (var db = new ErpContext())
            {

                // [check] Task id exists
                TaskDetail taskDetail = db.TaskDetail
                    .FirstOrDefault(x => x.TaskId == checkListItemEditModel.TaskId);
                if (taskDetail == null)
                {
                    throw new KeyNotFoundException("Task id does not exist");
                }
                checkList = db.CheckList
                    .FirstOrDefault(x => x.CheckListItemId == checkListItemEditModel.CheckListItemId);

                if (checkList == null)
                {
                    // [check] new task cannot be added once sprint approved
                    if(taskDetail.SprintId != null)
                        status = SprintManagementService.GetSprintById(taskDetail.SprintId)
                        .Status;
                    if (status != SStatus.Planning || status != SStatus.RequestForApproval)
                    {
                        throw new KeyNotFoundException("New Checklist cannot be added once sprint is approved");
                    }
                    // [case] creating new checklist
                    checkList = new CheckList();
                    checkList.CheckListItemId = GetNextAvailableId();
                    newCheckList = true;
                }
                else
                {
                    checkList.CheckListItemId = checkListItemEditModel.CheckListItemId;
                    
                    // get Sprint Status
                    TaskDetail taskDetailIndB = db.TaskDetail
                        .FirstOrDefault(x => x.TaskId == checkList.TaskId);
                    status = SprintManagementService.GetSprintById(taskDetailIndB.SprintId)
                        .Status; 
                }
                 
                    
                if (status != SStatus.Reviewed)
                {
                    checkList.ManagerComment = checkListItemEditModel.ManagerComment;
                }
                if (status != SStatus.Closed)
                {
                        checkList.UserComment = checkListItemEditModel.UserComment;
                        checkList.Status = checkListItemEditModel.Status.ToString();
                        checkList.Result = checkListItemEditModel.Result;
                }
                if (status != SStatus.Approved)
                {
                        checkList.Description = checkListItemEditModel.Description;
                        checkList.TaskId = checkListItemEditModel.TaskId;
                        checkList.WorstCase = checkListItemEditModel.WorstCase;
                        checkList.BestCase = checkListItemEditModel.BestCase;
                        checkList.ResultType = checkListItemEditModel.ResultType;
                        checkList.Essential = checkListItemEditModel.Essential;
                } 
               
                
                if ( newCheckList)
                {
                    db.CheckList.Add(checkList);
                    db.SaveChanges();
                   
                }
                else
                {
                    if(taskDetail.SprintId != null)
                        status = SprintManagementService.GetSprintById(taskDetail.SprintId)
                            .Status;
                    if (status != SStatus.Planning || status != SStatus.RequestForApproval)
                    {
                        SprintReportManagementService.UpdateSprintReportLineItem(GetSprintReportLineItemForCheckListitem(checkListItemEditModel));
                    }
                    
                    db.SaveChanges();
                }
            }

            return GetCheckListById(checkList.CheckListItemId);
        }

        
        public static SprintReportEditModel GetSprintReportLineItemForCheckListitem(CheckListItemEditModel checkListItem)
        {
            string sprintReportLineItemId =
                SprintReportManagementService.GetSprintreportLineItemIdForCheckListId(checkListItem.CheckListItemId);
            SprintReportEditModel sprintReportEditModel = SprintReportManagementService
                .GetSprintReportItemById(sprintReportLineItemId);
            sprintReportEditModel.ManagerComment = checkListItem.ManagerComment;
            sprintReportEditModel.Result = checkListItem.Result;
            sprintReportEditModel.UserComment = checkListItem.UserComment;
            return sprintReportEditModel;
        
        }
        


        private static string GetNextAvailableId()
        {
            using (var db = new ErpContext())
            {
                var a = db.CheckList
                    .Select(x => Convert.ToInt32(x.CheckListItemId))
                    .DefaultIfEmpty(0)
                    .Max();
                return Convert.ToString(a + 1);
            }
          
        }
        
        private static List<CheckListItemEditModel> GetCheckListForATaskId(string  taskId)
        {

            List<CheckListItemEditModel> checkListEditModels = new List<CheckListItemEditModel>();
            using (var db = new ErpContext())
            {
                List<string> checkList = db.CheckList
                    .Where(x => x.TaskId == taskId)
                    .Select(t => t.CheckListItemId)
                    .ToList();

                checkList.ForEach(
                    x => checkListEditModels.Add(
                        GetCheckListById(x)));

                return checkListEditModels;
                
            }
        }
        
        private static CheckListItemEditModel GetCheckListById(string checkListItemId)
        {
            using (var db = new ErpContext())
            {
                
                CheckList existingCheckList = db.CheckList
                    .FirstOrDefault(x => x.CheckListItemId == checkListItemId);
                if (existingCheckList == null)
                    return null;
                CheckListItemEditModel checkListItemEditModel = new CheckListItemEditModel()
                {
                    CheckListItemId = existingCheckList.CheckListItemId,
                    TaskId = existingCheckList.TaskId,
                    Description = existingCheckList.Description,
                    Status = (CStatus) Enum.Parse(typeof(CStatus), existingCheckList.Status, true),
                    Attachment = existingCheckList.Attachment,
                    WorstCase = existingCheckList.WorstCase,
                    BestCase = existingCheckList.BestCase,
                    ResultType = existingCheckList.ResultType,
                    Result = existingCheckList.Result,
                    Essential = existingCheckList.Essential,
                    UserComment = existingCheckList.UserComment,
                    ManagerComment = existingCheckList.ManagerComment
                    
                    
                };

                return checkListItemEditModel;
            }

        }


    }
}