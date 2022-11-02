using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using flexli_erp_webapi.DataModels;
using flexli_erp_webapi.EditModels;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;


namespace flexli_erp_webapi.Services
{
    public class CheckListManagementService
    {
        public static List<CheckListItemEditModel> GetCheckList(string taskId, ECheckListType type, int? pageIndex = null, int? pageSize = null)
        {
            if (pageIndex != null && pageSize != null)
            {
                return GetCheckListPageForTaskId(taskId, type, (int) pageIndex, (int) pageSize);
            }
            return GetCheckListForTypeId(taskId,type);
        }
        
        private static List<CheckListItemEditModel> GetCheckListPageForTaskId(string taskId, ECheckListType type, int pageIndex, int pageSize)
        {
            List<CheckListItemEditModel> checkListEditModels = new List<CheckListItemEditModel>();
            using (var db = new ErpContext())
            {
                if (pageIndex <= 0 || pageSize <= 0)
                    throw new ArgumentException("Incorrect value for pageIndex or pageSize");
                
                // skip take logic
                List<string> checkList = db.CheckList
                    .Where(x => x.TypeId == taskId && x.CheckListType == type.ToString())
                    .Select(t => t.CheckListItemId)
                    .OrderByDescending(t=>Convert.ToInt32(t))
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                if (checkList.Count == 0)
                {
                    throw new ArgumentException("Incorrect value for pageIndex or pageSize");
                }
                checkList.ForEach(
                    x => checkListEditModels.Add(
                        GetCheckListById(x)));

                return checkListEditModels;

            }
        }

        public static CheckListItemEditModel CreateOrUpdateCheckListItem(CheckListItemEditModel checkListItemEditModel)
        {

            if (checkListItemEditModel.CheckListType == ECheckListType.Template)
            {
                return CreateOrUpdateCheckListInDb(checkListItemEditModel);
            }
            // If checklist exist - get Sprint Status from DB
            var checkList = GetCheckListById(checkListItemEditModel.CheckListItemId);
            var task = (checkList != null)?TaskManagementService.GetTaskById(checkList.TypeId): null;
            var sprint = (task != null) ?SprintManagementService.GetSprintById(
                task.SprintId): null;
            
            // [check] Checklist can added only if sprint is in planning stage
            // If checklist does not exist - get Sprint Status from checklist param in function
            if (checkList == null)
            {
                task = (checkListItemEditModel.TypeId != null)?TaskManagementService.GetTaskById(checkListItemEditModel.TypeId)
                    : throw new KeyNotFoundException("Task Id not mentioned");
                sprint = (task != null) ?SprintManagementService.GetSprintById(
                    task.SprintId): throw new KeyNotFoundException("Task does not exist");
                
                if (sprint != null)
                {
                    if (sprint.Status != SStatus.Planning)
                    {
                        throw new KeyNotFoundException("Checklist can added only if sprint is in planning stage");  
                    } 
                }
                
            }
            // [Check]: If result type is numeric then best and worst case and result params cannot be null

            if (checkListItemEditModel.ResultType == CResultType.Numeric)
            {
                if (!checkListItemEditModel.BestCase.HasValue || !checkListItemEditModel.WorstCase.HasValue)
                {
                    throw new KeyNotFoundException("If result type is numeric then best and worst case params cannot be null");  
                }

                if (checkListItemEditModel.Result.Length == 0)
                {
                    checkListItemEditModel.Result = "0";
                }
            }
            // [check] Checklist params could be modified based upon sprint state
            var updatedCheckList = ApplySprintStatusBasedCheck(checkListItemEditModel, sprint != null ?sprint.Status: SStatus.Planning);
           
        
            
            updatedCheckList = CreateOrUpdateCheckListInDb(updatedCheckList);
             
             //[Action]: Update in Sprint report if Status Approved
             if (sprint != null ? sprint.Approved: false)
             {
                 SprintReportManagementService.UpdateSprintReportLineItem(GetSprintReportLineItemForCheckListitem(updatedCheckList));
             }

             return updatedCheckList;
        }

        private static CheckListItemEditModel ApplySprintStatusBasedCheck(CheckListItemEditModel checkListItemEditModel, SStatus sprintStatus)
        {
            var closureStatus = new List<string> { SStatus.Closed.ToString(), SStatus.RequestForClosure.ToString() };
            CheckListItemEditModel editCheckList = GetCheckListById(checkListItemEditModel.CheckListItemId);
            if (editCheckList == null)
            {
                editCheckList = new CheckListItemEditModel();
            }
            if (! closureStatus.Contains(sprintStatus.ToString()))
            {
                editCheckList.UserComment = checkListItemEditModel.UserComment;
                editCheckList.Status = checkListItemEditModel.Status;
                editCheckList.Result = checkListItemEditModel.Result;
            }
            if (sprintStatus == SStatus.Planning)
            {
                editCheckList.Description = checkListItemEditModel.Description;
                editCheckList.TypeId = checkListItemEditModel.TypeId;
                editCheckList.WorstCase = checkListItemEditModel.WorstCase;
                editCheckList.BestCase = checkListItemEditModel.BestCase;
                editCheckList.ResultType = checkListItemEditModel.ResultType;
                editCheckList.Essential = checkListItemEditModel.Essential;
            }

            return editCheckList;
        }

       

        public static void DeleteCheckListItem(string checkListItemId)
        {
            using (var db = new ErpContext())
            {
                

                // Get Selected TasK
                CheckList existingCheckList = db.CheckList
                    .FirstOrDefault(x => x.CheckListItemId == checkListItemId);
                



                if (existingCheckList != null && existingCheckList.CheckListType  == ECheckListType.Task.ToString())
                {
                    var sprintId = db.TaskDetail
                        .FirstOrDefault(x => x.TaskId == existingCheckList.TypeId)
                        .SprintId
                        ;

                    // [Check]: Sprint should be in Planning Stage
                    if (sprintId != null)
                    {
                        if (SprintManagementService.CheckStatus(sprintId) != SStatus.Planning)
                        {
                            throw new ConstraintException("Checklist cannot be deleted, sprint is already approved");
                        }  
                    }
                   
                    
                    
                }
                
                db.CheckList.Remove(existingCheckList);
                db.SaveChanges();


            }
        }

        
      

        
        private static CheckListItemEditModel CreateOrUpdateCheckListInDb(CheckListItemEditModel checkListItemEditModel)
        {
            CheckList checkList;
            using (var db = new ErpContext())
            {

                Boolean newCheckList = false;
                
                 checkList = db.CheckList
                    .FirstOrDefault(x => x.CheckListItemId == checkListItemEditModel.CheckListItemId);

                if (checkList == null)
                {
                    // [case] creating new checklist
                    checkList = new CheckList();
                    checkList.CheckListItemId = GetNextAvailableId();
                    newCheckList = true;
                }

                checkList.Description = checkListItemEditModel.Description; 
                checkList.TypeId = checkListItemEditModel.TypeId;
                checkList.WorstCase = checkListItemEditModel.WorstCase; 
                checkList.BestCase = checkListItemEditModel.BestCase;
                checkList.ResultType = checkListItemEditModel.ResultType.ToString();
                checkList.Essential = checkListItemEditModel.Essential; 
                checkList.UserComment = checkListItemEditModel.UserComment; 
                checkList.Status = checkListItemEditModel.Status.ToString(); 
                checkList.Result = checkListItemEditModel.Result; 
                checkList.ManagerComment = checkListItemEditModel.ManagerComment;
                checkList.CheckListType = checkListItemEditModel.CheckListType.ToString();
                
               
                
                if (newCheckList)
                {
                    db.CheckList.Add(checkList);
                    db.SaveChanges();
                   
                }
                else
                {
                    db.SaveChanges();
                }
            }

            return GetCheckListById(checkList.CheckListItemId);
        }

        
        public static SprintReportEditModel GetSprintReportLineItemForCheckListitem(CheckListItemEditModel checkListItem)
        {
            string sprintReportLineItemId =
                SprintReportManagementService.GetSprintreportLineItemIdForCheckListId(checkListItem.CheckListItemId);
            if (sprintReportLineItemId == null)
            {
                throw new KeyNotFoundException("Sprint report lineitem does not exist for checklist item: " +
                                               checkListItem.CheckListItemId);
            }
            SprintReportEditModel sprintReportEditModel = SprintReportManagementService
                .GetSprintReportItemById(sprintReportLineItemId);
            sprintReportEditModel.ManagerComment = checkListItem.ManagerComment;
            sprintReportEditModel.Result = checkListItem.Result;
            sprintReportEditModel.UserComment = checkListItem.UserComment;
            sprintReportEditModel.Status = checkListItem.Status;
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
        
        private static List<CheckListItemEditModel> GetCheckListForTypeId(string  typeId, ECheckListType type)
        {

            List<CheckListItemEditModel> checkListEditModels = new List<CheckListItemEditModel>();
            using (var db = new ErpContext())
            {
                List<string> checkList = db.CheckList
                    .Where(x => x.TypeId == typeId && x.CheckListType == type.ToString())
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
                    TypeId = existingCheckList.TypeId,
                    Description = existingCheckList.Description,
                    Status = (CStatus) Enum.Parse(typeof(CStatus), existingCheckList.Status, true),
                    WorstCase = existingCheckList.WorstCase,
                    BestCase = existingCheckList.BestCase,
                    ResultType = existingCheckList.ResultType != null?
                        (CResultType) Enum.Parse(typeof(CResultType), existingCheckList.ResultType, true): CResultType.File,
                    Result = existingCheckList.Result,
                    Essential = existingCheckList.Essential,
                    UserComment = existingCheckList.UserComment,
                    ManagerComment = existingCheckList.ManagerComment,
                    CheckListType = (ECheckListType) Enum.Parse(typeof(ECheckListType), existingCheckList.CheckListType, true),
                    
                    
                };

                return checkListItemEditModel;
            }

        }


        public static CheckListItemEditModel AddNewChecklistItemForTaskWithNoChecklist(string taskId)
        {
            using (var db = new ErpContext())
            {
                TaskDetail task = db.TaskDetail
                    .FirstOrDefault(x => x.TaskId == taskId);

                CheckListItemEditModel dummyNewChecklistItem = new CheckListItemEditModel()
                {
                    CheckListItemId = "newChecklist",
                    Description = task.Description,
                    TypeId = task.TaskId,
                    Status = CStatus.NotCompleted,
                    WorstCase = 0,
                    BestCase = 0,
                    ResultType = CResultType.Boolean,
                    Essential = true,
                    CheckListType = ECheckListType.Task
                };

                return CreateOrUpdateCheckListInDb(dummyNewChecklistItem);
            }
        }
    }
}