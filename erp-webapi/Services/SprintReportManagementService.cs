using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using flexli_erp_webapi.DataModels;
using flexli_erp_webapi.EditModels;

namespace flexli_erp_webapi.Services
{
    public class SprintReportManagementService
    {
       
        public static List<SprintReportEditModel> GetSprintReportForSprint(string sprintId, int? pageIndex = null, int? pageSize = null)
        {
          
            
            List<SprintReportEditModel> sprintReport = new List<SprintReportEditModel>();
            List<string> sprintReportLineItemIds = new List<string>();
            
            sprintReportLineItemIds = GetSprintReportLineItemIdsForSprint(sprintId, pageIndex, pageSize);

            if (sprintReportLineItemIds.Count == 0)
            {
                return sprintReport;
            }
            
            sprintReportLineItemIds.ForEach(x =>
            {
                sprintReport.Add(GetSprintReportItemById(x));
            });
            return sprintReport;
        }

        private static List<string> GetSprintReportLineItemIdsForSprint(string sprintId, int? pageIndex = null, int? pageSize = null)
        {
            List<string> sprintReportLineItemIds = new List<string>();
            using (var db = new ErpContext())
            {
                // [Check] : Pagination
                if (pageIndex != null && pageSize != null)
                {
                    if (pageIndex <= 0 || pageSize <= 0)
                        throw new ArgumentException("Incorrect value for pageIndex or pageSize");
                
                    // skip take logic
                    sprintReportLineItemIds = db.SprintReport
                        .Where(x => x.SprintId == sprintId)
                        .Select(y => y.SprintReportLineItemId).AsEnumerable()
                        .OrderByDescending(Convert.ToInt32)
                        .Skip(((int) pageIndex - 1) * (int) pageSize)
                        .Take((int) pageSize)
                        .ToList();

                    if (sprintReportLineItemIds.Count == 0)
                    {
                        throw new ArgumentException("Incorrect value for pageIndex or pageSize");
                    }

                    return sprintReportLineItemIds;
                }
                
                sprintReportLineItemIds = db.SprintReport
                    .Where(x => x.SprintId == sprintId)
                    .Select(y => y.SprintReportLineItemId).AsEnumerable()
                    .OrderByDescending(Convert.ToInt32)
                    .ToList();


                return sprintReportLineItemIds;
            }
        }
        public static SprintReportEditModel GetSprintReportItemById(string sprintReportLineItemId)
        {
            SprintReport sprintReport;
            using (var db = new ErpContext())
            {
                sprintReport = db.SprintReport
                    .FirstOrDefault(x => x.SprintReportLineItemId == sprintReportLineItemId);

                if (sprintReport == null)
                {
                    throw new KeyNotFoundException("Error in finding sprintReportLineItem due to invalid Id.");
                }

                return GetSprintReportItemByIdFromDb(sprintReport);
            }
        }

        private static SprintReportEditModel GetSprintReportItemByIdFromDb(SprintReport sprintReport)
        {
            SprintReportEditModel sprintReportEditModel = new SprintReportEditModel()
            {
                SprintReportLineItemId = sprintReport.SprintReportLineItemId,
                TaskId = sprintReport.TaskId,
                CheckListItemId = sprintReport.CheckListItemId,
                Description = sprintReport.Description,
                ResultType = (CResultType) Enum.Parse(typeof(CResultType), sprintReport.ResultType,true),
                Result = sprintReport.Result,
                UserComment = sprintReport.UserComment,
                ManagerComment = sprintReport.ManagerComment,
                Approved = (SApproved) Enum.Parse(typeof(SApproved),sprintReport.Approved,true),
                Status = (CStatus) Enum.Parse(typeof(CStatus), sprintReport.Status, true),
                WorstCase = sprintReport.WorstCase,
                BestCase = sprintReport.BestCase,
                Score = sprintReport.Score,
                SprintId = sprintReport.SprintId
            };

            return sprintReportEditModel;
        }

        public static SprintReportEditModel UpdateSprintReportLineItem(SprintReportEditModel sprintReportEditModel)
        {
            SprintReport sprintReport;
            
            
            using (var db = new ErpContext())
            {

                sprintReport = db.SprintReport
                    .FirstOrDefault(x => x.SprintId == sprintReportEditModel.SprintId && x.CheckListItemId == sprintReportEditModel.CheckListItemId)
                    ?? throw new KeyNotFoundException("Sprint report Id does not exist: " +
                                                      sprintReportEditModel.SprintId);
                switch (SprintManagementService.GetSprintById(sprintReportEditModel.SprintId).Closed)
                {
                    case false:
                        sprintReport.Result = sprintReportEditModel.Result;
                        sprintReport.UserComment = sprintReportEditModel.UserComment;
                        sprintReport.Status = sprintReportEditModel.Status.ToString();

                        db.SaveChanges();
                        break;
                }

            }

            return GetSprintReportItemById(sprintReport.SprintReportLineItemId);
        }
        
     
        
        public static SprintReportEditModel ReviewCheckList(SprintReportEditModel sprintReportEditModel, string approverId)
        {
            // If checklist exist - get Sprint Status from DB
            var sprintReportLineItem = GetSprintReportItemById(sprintReportEditModel.SprintReportLineItemId);
            if (sprintReportLineItem == null)
            {
                throw new KeyNotFoundException("Sprint report does not exist" + sprintReportEditModel.SprintReportLineItemId);
            }

            var sprint = SprintManagementService.GetSprintById(sprintReportLineItem.SprintId);
            
            // [check] Approver id is valid

            if (!ProfileManagementService.CheckManagerValidity(sprint.Owner, approverId))
            {
                throw new ConstraintException("Not valid approver Id: " + approverId);
            }
                
                
            //[Check]: Sprint is not reviewed
            if (sprint.Status != SStatus.Reviewed)
            {
                using (var db = new ErpContext())
                {

                    SprintReport sprintReport = db.SprintReport
                        .FirstOrDefault(x => x.SprintReportLineItemId == sprintReportEditModel.SprintReportLineItemId);
                    
                    if (sprintReportEditModel.ManagerComment != null)
                    {
                        sprintReport.ManagerComment = sprintReportEditModel.ManagerComment;
                    }
                    
                    if (sprintReportEditModel.Approved != null)
                    {
                        sprintReport.Approved = sprintReportEditModel.Approved.ToString();
                    }
                   
                 
                    db.SaveChanges();
                }
            }

            return GetSprintReportItemById(sprintReportEditModel.SprintReportLineItemId);
        }
        public static void AddSprintReportLineItem(string sprintId)
        {
            using (var db = new ErpContext())
            {
                SprintEditModel sprint = SprintManagementService.GetSprintById(sprintId);

                if (sprint == null)
                {
                    throw new KeyNotFoundException("invalid sprint id, sprint doesn't exist");
                }
                
                List<string> tasks = db.TaskDetail
                    .Where(task => task.SprintId == sprintId)
                    .Select(task => task.TaskId)
                    .ToList();

                foreach (var task in tasks)
                {
                    List<CheckListItemEditModel> checkListItems = CheckListManagementService.GetCheckList(task, ECheckListType.Task);
                        
                    // It is necessary to have atleast one checklist for each task
                    // in order to make sprint - task - checklist pair entry
                    // So if task with no checklist added to sprint,
                    // create a new dummy checklist for that task

                    if (checkListItems.Count == 0)
                    {
                        CheckListItemEditModel dummyNewChecklistItem = CheckListManagementService.AddNewChecklistItemForTaskWithNoChecklist(task);
                        checkListItems.Add(dummyNewChecklistItem);
                    }

                    foreach (var checkListItem in checkListItems)
                    {
                        SprintReport sprintReport = new SprintReport()
                        {
                            SprintReportLineItemId = GetNextAvailableId(),
                            SprintId = sprintId,
                            TaskId = task,
                            CheckListItemId = checkListItem.CheckListItemId,
                            Description = checkListItem.Description,
                            ResultType = checkListItem.ResultType.ToString(),
                            UserComment = checkListItem.UserComment,
                            Approved = SApproved.NoAction.ToString(),
                            Status = CStatus.NotCompleted.ToString(),
                            WorstCase = checkListItem.WorstCase,
                            BestCase = checkListItem.BestCase,
                            Score = 0
                        };

                        db.SprintReport.Add(sprintReport);
                        db.SaveChanges();
                    }
                }
            }
        }
        
        private static string GetNextAvailableId()
        {
            using (var db = new ErpContext())
            {
                var a = db.SprintReport
                    .Select(x => Convert.ToInt32(x.SprintReportLineItemId))
                    .DefaultIfEmpty(0)
                    .Max();
                return Convert.ToString(a + 1);
            }
          
        }

        public static bool AllSprintReportLineItemsStatusNotNotCompleted(string sprintId)
        {
            using (var db = new ErpContext())
            {
                List<string> status = db.SprintReport
                    .Where(x => x.SprintId == sprintId)
                    .Select(x => x.Status)
                    .ToList();

                int flag = 0;
                status.ForEach(x =>
                {
                    if (x == CStatus.NotCompleted.ToString())
                    {
                        flag = 1;
                    }
                });

                if (flag == 1)
                    return false;

                return true;
            }
        }

        public static void PublishActualScoresForSprintReport(string sprintId)
        {
            Sprint sprint;
            List<SprintReport> sprintReports;
            using (var db = new ErpContext())
            {
              
                sprintReports = db.SprintReport
                    .Where(x => x.SprintId == sprintId)
                    .ToList();
                
                sprintReports.ForEach(x =>
                {
                    if (x.Approved == SApproved.NoAction.ToString())
                    {
                        x.Approved = "True";
                        db.SaveChanges();
                    }

                });
                sprintReports.ForEach(x =>
                {
                    CheckList checkList = db.CheckList
                        .FirstOrDefault(s => s.CheckListItemId == x.CheckListItemId);

                    if (x.Approved == SApproved.False.ToString() && checkList.Essential)
                    {
                        x.Score = 0;
                        db.SaveChanges();
                    }
                    
                    else if (x.Approved == SApproved.False.ToString() && !checkList.Essential)
                    {
                        if (x.Score != 0)
                        {
                            // TaskDetail taskDetail = db.TaskDetail
                            //     .FirstOrDefault(z => z.TaskId==x.TaskId);
                            //
                            // taskDetail.AcceptanceCriteria--;
                            // db.SaveChanges();
                            //
                            // TaskManagementService.UpdateProvisionalTaskScore(x.SprintId);
                        }
                    }

                });

            }
        }

        public static string GetSprintreportLineItemIdForCheckListId(string checkListItemId)
        {
            using (var db = new ErpContext())
            {
                
                var sprintReport = db.SprintReport
                    .Where(x => x.CheckListItemId == checkListItemId)
                    .ToList();
                // [Check] Multiple sprint report line item can have same checklist Item id. Use one with  sprint status not closed
                var reqSprintReport = sprintReport.Find(x => ! SprintManagementService
                    .GetSprintById(x.SprintId)
                    .Closed);


                return reqSprintReport is null? null: reqSprintReport.SprintReportLineItemId ;
            }
        }

        public static void UpdateProvisionalScoreInSprintReport(string sprintId)
        {
            List<SprintReport> sprintReports;
            using (var db = new ErpContext())
            {
                sprintReports = db.SprintReport
                    .Where(x => x.SprintId == sprintId)
                    .ToList();
                
                sprintReports.ForEach(sprintReport =>
                {
                    if (sprintReport.Status == CStatus.Completed.ToString())
                    {
                        if(sprintReport.ResultType==CResultType.Numeric.ToString() && Convert.ToInt32(sprintReport.Result)>=sprintReport.WorstCase && Convert.ToInt32(sprintReport.Result) <= sprintReport.BestCase)
                        {
                            sprintReport.Score = 1;
                            db.SaveChanges();
                        }

                        if (sprintReport.ResultType == CResultType.Boolean.ToString() && sprintReport.Result == "true")
                        {
                            sprintReport.Score = 1;
                            db.SaveChanges();
                        }

                        if (sprintReport.ResultType == CResultType.File.ToString() && sprintReport.Result != null)
                        {
                            sprintReport.Score = 1;
                            db.SaveChanges();
                        }
                    }
                });
            }
        }
    }
}