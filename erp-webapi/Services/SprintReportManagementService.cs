using System;
using System.Collections.Generic;
using System.Linq;
using flexli_erp_webapi.DataModels;
using flexli_erp_webapi.EditModels;

namespace flexli_erp_webapi.Services
{
    public class SprintReportManagementService
    {
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
                ResultType = sprintReport.ResultType,
                Result = sprintReport.Result,
                UserComment = sprintReport.UserComment,
                ManagerComment = sprintReport.ManagerComment,
                Approved = sprintReport.Approved,
                Status = sprintReport.Status,
                WorstCase = sprintReport.WorstCase,
                BestCase = sprintReport.BestCase,
                Score = sprintReport.Score
            };

            return sprintReportEditModel;
        }

        public static SprintReportEditModel UpdateSprintReportLineItem(SprintReportEditModel sprintReportEditModel, string approverId)
        {
            SprintReport sprintReport;
            using (var db = new ErpContext())
            {
                var userId = db.Sprint
                    .Where(x => x.SprintId == sprintReportEditModel.SprintId)
                    .Select(x => x.Owner)
                    .ToString();

                // Case: if not a valid manager
                if (!ProfileManagementService.FindValidManager(userId, approverId))
                {
                    throw new ArgumentException("approver Id is not allowed to make changes");
                }

                sprintReport = db.SprintReport
                    .FirstOrDefault(x => x.SprintId == sprintReportEditModel.SprintId && x.CheckListItemId == sprintReportEditModel.CheckListItemId);

                switch (SprintManagementService.CheckStatus(sprintReportEditModel.SprintId))
                {
                    case "approved":
                        sprintReport.Result = sprintReportEditModel.Result;
                        sprintReport.UserComment = sprintReportEditModel.UserComment;
                        sprintReport.ManagerComment = sprintReportEditModel.ManagerComment;
                        sprintReport.Approved = sprintReportEditModel.Approved;

                        db.SaveChanges();
                        break;
                    
                    case "requestforclosure":
                        sprintReport.Result = sprintReportEditModel.Result;
                        sprintReport.UserComment = sprintReportEditModel.UserComment;
                        sprintReport.ManagerComment = sprintReportEditModel.ManagerComment;

                        db.SaveChanges();
                        break;
                    
                    case "closed":
                        sprintReport.ManagerComment = sprintReportEditModel.ManagerComment;

                        db.SaveChanges();
                        break;
                }
            }

            return GetSprintReportItemById(sprintReport.SprintReportLineItemId);
        }
        
        public static void ApproveSprintReportLineItems(string sprintId)
        {
            using (var db = new ErpContext())
            {
                SprintReport sprintReport;
                List<string> sprintReportLineItemIds = db.SprintReport
                    .Where(x => x.SprintId == sprintId)
                    .Select(x => x.SprintReportLineItemId)
                    .ToList();
                
                sprintReportLineItemIds.ForEach(x=>
                    {
                        sprintReport = db.SprintReport.FirstOrDefault(s => s.SprintReportLineItemId == x);
                        sprintReport.Approved = "true";
                        db.SaveChanges();
                    }
                    
                );
            }
        }
        
        public static void AddSprintReportLineItem(string sprintId)
        {
            using (var db = new ErpContext())
            {
                List<string> tasks = db.TaskDetail
                    .Where(x => x.SprintId == sprintId)
                    .Select(x => x.TaskId)
                    .ToList();
                
                tasks.ForEach(x =>
                {
                    List<string> checkListItemIds = db.CheckList
                        .Where(s => s.TaskId == x)
                        .Select(s => s.CheckListItemId)
                        .ToList();

                    checkListItemIds.ForEach(y =>
                    {
                        CheckList checkList = db.CheckList
                            .FirstOrDefault(z => z.CheckListItemId == y);
                        
                        SprintReport sprintReport = new SprintReport()
                        {
                            SprintReportLineItemId = GetNextAvailableId(),
                            SprintId = sprintId,
                            TaskId = x,
                            CheckListItemId = checkList.CheckListItemId,
                            Description = checkList.Description,
                            ResultType = checkList.ResultType,
                            UserComment = checkList.UserComment,
                            Approved = "no action",
                            Status = CStatus.notCompleted.ToString(),
                            WorstCase = checkList.WorstCase,
                            BestCase = checkList.BestCase,
                            Score = 0
                        };

                        db.SprintReport.Add(sprintReport);
                        db.SaveChanges();
                    });
                });
                
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
    }
}