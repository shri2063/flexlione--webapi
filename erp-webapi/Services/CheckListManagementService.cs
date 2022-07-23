using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using flexli_erp_webapi.DataModels;
using flexli_erp_webapi.EditModels;


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
            
            using (var db = new ErpContext())
            {
                checkList = db.CheckList
                    .FirstOrDefault(x => x.CheckListItemId == checkListItemEditModel.CheckListItemId);


                if (checkList != null) // update
                {
                    TaskDetail taskDetail = db.TaskDetail
                        .FirstOrDefault(x => x.TaskId == checkListItemEditModel.TaskId);

                    if (taskDetail.SprintId == null)
                    {
                        checkList.CheckListItemId = checkListItemEditModel.CheckListItemId;
                        checkList.Status = checkListItemEditModel.Status.ToString().ToLower();
                        checkList.TaskId = checkListItemEditModel.TaskId;
                        checkList.Attachment = checkListItemEditModel.Attachment;
                        checkList.Description = checkListItemEditModel.Description;
                        checkList.WorstCase = checkListItemEditModel.WorstCase;
                        checkList.BestCase = checkListItemEditModel.BestCase;
                        checkList.ResultType = checkListItemEditModel.ResultType;
                        checkList.Essential = checkListItemEditModel.Essential;
                        checkList.Result = checkListItemEditModel.Result;
                        checkList.UserComment = checkListItemEditModel.UserComment;
                    
                        db.SaveChanges();
                    }
                    else
                    {
                        switch (SprintManagementService.CheckStatus(taskDetail.SprintId))
                        {
                            case "planning":
                                checkList.CheckListItemId = checkListItemEditModel.CheckListItemId;
                                checkList.TaskId = checkListItemEditModel.TaskId;
                                checkList.Attachment = checkListItemEditModel.Attachment;
                                checkList.Description = checkListItemEditModel.Description;
                                checkList.WorstCase = checkListItemEditModel.WorstCase;
                                checkList.BestCase = checkListItemEditModel.BestCase;
                                checkList.ResultType = checkListItemEditModel.ResultType;
                                checkList.Essential = checkListItemEditModel.Essential;

                                db.SaveChanges();
                                break;
                            
                            case "requestforapproval":
                                checkList.CheckListItemId = checkListItemEditModel.CheckListItemId;
                                checkList.TaskId = checkListItemEditModel.TaskId;
                                checkList.Attachment = checkListItemEditModel.Attachment;
                                checkList.Description = checkListItemEditModel.Description;
                                checkList.WorstCase = checkListItemEditModel.WorstCase;
                                checkList.BestCase = checkListItemEditModel.BestCase;
                                checkList.ResultType = checkListItemEditModel.ResultType;
                                checkList.Essential = checkListItemEditModel.Essential;

                                db.SaveChanges();
                                break;
                            
                            case "approved":
                                checkList.CheckListItemId = checkListItemEditModel.CheckListItemId;
                                checkList.Status = checkListItemEditModel.Status.ToString().ToLower();
                                checkList.TaskId = checkListItemEditModel.TaskId;
                                checkList.Attachment = checkListItemEditModel.Attachment;
                                checkList.Result = checkListItemEditModel.Result;
                                checkList.UserComment = checkListItemEditModel.UserComment;

                                db.SaveChanges();
                                
                                // UpdateResultAndCommentInSprintReport(checkList.CheckListItemId, checkListItemEditModel);
                                break;
                            
                            case "requestforclosure":
                                checkList.CheckListItemId = checkListItemEditModel.CheckListItemId;
                                checkList.Status = checkListItemEditModel.Status.ToString().ToLower();
                                checkList.TaskId = checkListItemEditModel.TaskId;
                                checkList.Attachment = checkListItemEditModel.Attachment;
                                checkList.Result = checkListItemEditModel.Result;
                                checkList.UserComment = checkListItemEditModel.UserComment;

                                db.SaveChanges();
                                
                                // UpdateResultAndCommentInSprintReport(checkList.CheckListItemId, checkListItemEditModel);
                                break;
                            
                            case "closed":
                                checkList.CheckListItemId = checkListItemEditModel.CheckListItemId;
                                checkList.Status = checkListItemEditModel.Status.ToString().ToLower();
                                checkList.TaskId = checkListItemEditModel.TaskId;
                                checkList.Attachment = checkListItemEditModel.Attachment;

                                db.SaveChanges();
                                break;
                        }
                    }
                    
                }
                else
                {
                    var sprintId = db.TaskDetail
                        .Where(x => x.TaskId == checkListItemEditModel.TaskId)
                        .Select(x => x.SprintId)
                        .ToString();

                    if (sprintId == null)
                    {
                        checkList = new CheckList
                        {
                            CheckListItemId = GetNextAvailableId(),
                            Status = checkListItemEditModel.Status.ToString().ToLower(),
                            TaskId = checkListItemEditModel.TaskId,
                            Attachment = checkListItemEditModel.Attachment,
                            Description = checkListItemEditModel.Description,
                            WorstCase = checkListItemEditModel.WorstCase,
                            BestCase = checkListItemEditModel.BestCase,
                            ResultType = checkListItemEditModel.ResultType,
                            Essential = checkListItemEditModel.Essential,
                            Result = checkListItemEditModel.Result,
                            UserComment = checkListItemEditModel.UserComment
                        
                        };
                        db.CheckList.Add(checkList);
                        db.SaveChanges();
                    }

                    else if (SprintManagementService.CheckApproved(sprintId))
                    {
                        checkList = new CheckList
                        {
                            CheckListItemId = GetNextAvailableId(),
                            Status = checkListItemEditModel.Status.ToString().ToLower(),
                            TaskId = checkListItemEditModel.TaskId,
                            Attachment = checkListItemEditModel.Attachment,
                        
                        };
                        db.CheckList.Add(checkList);
                        db.SaveChanges();
                    }
                    
                    else if (!SprintManagementService.CheckApproved(sprintId))
                    {
                        checkList = new CheckList
                        {
                            CheckListItemId = GetNextAvailableId(),
                            Status = CStatus.notCompleted.ToString(),
                            TaskId = checkListItemEditModel.TaskId,
                            Attachment = checkListItemEditModel.Attachment,
                            Description = checkListItemEditModel.Description,
                            WorstCase = checkListItemEditModel.WorstCase,
                            BestCase = checkListItemEditModel.BestCase,
                            ResultType = checkListItemEditModel.ResultType,
                            Essential = checkListItemEditModel.Essential
                        
                        };
                        db.CheckList.Add(checkList);
                        db.SaveChanges();
                    }
                }
                
                // AddOrUpdateCheckListForSprint(checkList.CheckListItemId, checkListItemEditModel);
            }

            return GetCheckListById(checkList.CheckListItemId);
        }

        private static void AddOrUpdateCheckListForSprint(string checklistItemId, CheckListItemEditModel checkListItemEditModel)
        {
            using (var db = new ErpContext())
            {
                var sprintId = db.TaskDetail
                    .Where(x => x.TaskId == checkListItemEditModel.TaskId)
                    .Select(x => x.SprintId)
                    .ToString();

                CheckList checkList = db.CheckList
                    .FirstOrDefault(x => x.CheckListItemId == checklistItemId);
                
                if (SprintManagementService.CheckApproved(sprintId) && !SprintManagementService.CheckClosed(sprintId))
                {
                    checkList.Result = checkListItemEditModel.Result;
                    checkList.UserComment = checkListItemEditModel.UserComment;

                    db.SaveChanges();
                    UpdateResultAndCommentInSprintReport(checklistItemId, checkListItemEditModel);
                }

                if (!SprintManagementService.CheckApproved(sprintId))
                {
                    checkList.Description = checkListItemEditModel.Description;
                    checkList.WorstCase = checkListItemEditModel.WorstCase;
                    checkList.BestCase = checkListItemEditModel.BestCase;
                    checkList.ResultType = checkListItemEditModel.ResultType;
                    checkList.Essential = checkListItemEditModel.Essential;
                    checkList.Result = checkListItemEditModel.Result; 
                    checkList.UserComment = checkListItemEditModel.UserComment;
                
                    db.SaveChanges();
                }

            }
        }

        private static void UpdateResultAndCommentInSprintReport(string checklistItemId, CheckListItemEditModel checkListItemEditModel)
        {
            using (var db = new ErpContext())
            {
                SprintReport sprintReport = db.SprintReport
                    .FirstOrDefault(x => x.CheckListItemId == checklistItemId);

                sprintReport.Result = checkListItemEditModel.Result;
                sprintReport.UserComment = checkListItemEditModel.UserComment;

                db.SaveChanges();
            }
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
                    
                    
                };

                return checkListItemEditModel;
            }

        }


    }
}