﻿using System;
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
                ResultType = (CResultType) Enum.Parse(typeof(CResultType), sprintReport.ResultType,true),
                Result = sprintReport.Result,
                UserComment = sprintReport.UserComment,
                ManagerComment = sprintReport.ManagerComment,
                Approved = sprintReport.Approved,
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
                    .FirstOrDefault(x => x.SprintId == sprintReportEditModel.SprintId && x.CheckListItemId == sprintReportEditModel.CheckListItemId);

                switch (SprintManagementService.CheckStatus(sprintReportEditModel.SprintId))
                {
                    case SStatus.Approved:
                        sprintReport.Result = sprintReportEditModel.Result;
                        sprintReport.UserComment = sprintReportEditModel.UserComment;
                        sprintReport.Status = sprintReportEditModel.Status.ToString();

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
                    .Where(task => task.SprintId == sprintId)
                    .Select(task => task.TaskId)
                    .ToList();

                foreach (var task in tasks)
                {
                    List<CheckListItemEditModel> checkListItems = CheckListManagementService.GetCheckList(task, "items");

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
                            Approved = "no action",
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

        public static bool AllSprintReportLineItemsStatusNotNoChange(string sprintId)
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
                    if (x == "NotCompleted")
                    {
                        flag = 1;
                    }
                });

                if (flag == 1)
                    return false;

                return true;
            }
        }

        public static void PublishActualScores(string sprintId)
        {
            Sprint sprint;
            List<SprintReport> sprintReports;
            using (var db = new ErpContext())
            {
                sprint = db.Sprint
                    .FirstOrDefault(x => x.SprintId == sprintId);
                sprintReports = db.SprintReport
                    .Where(x => x.SprintId == sprintId)
                    .ToList();
                
                sprintReports.ForEach(x =>
                {
                    if (x.Approved == "no action")
                    {
                        x.Approved = "true";
                        db.SaveChanges();
                    }

                });
                sprintReports.ForEach(x =>
                {
                    CheckList checkList = db.CheckList
                        .FirstOrDefault(s => s.CheckListItemId == x.CheckListItemId);

                    if (x.Approved == "false" && checkList.Essential)
                    {
                        x.Score = 0;
                        db.SaveChanges();
                    }
                    
                    else if (x.Approved == "false" && !checkList.Essential)
                    {
                        if (x.Score > 0)
                        {
                            TaskDetail taskDetail = db.TaskDetail
                                .FirstOrDefault(z => z.TaskId==x.TaskId);

                            taskDetail.AcceptanceCriteria--;
                            db.SaveChanges();
                            
                            TaskManagementService.UpdateProvisionalTaskScore(x.SprintId);
                        }
                    }

                });
                
                List<int?> taskScores = db.TaskDetail
                    .Where(x => x.SprintId == sprintId)
                    .Select(x => x.Score)
                    .ToList();
                
                // Provisional score of sprint
                sprint.Score = taskScores.Sum();
                
                db.SaveChanges();
                
                
            }
        }

        public static string GetSprintreportLineItemIdForCheckListId(string checkListItemId)
        {
            using (var db = new ErpContext())
            {
                var sprintReport = db.SprintReport
                    .FirstOrDefault(x => x.CheckListItemId == checkListItemId);

                if (sprintReport == null)
                {
                    return null;
                }

                return sprintReport.SprintReportLineItemId;
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