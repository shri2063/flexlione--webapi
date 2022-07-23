using System;
using System.Collections.Generic;
using System.Linq;
using flexli_erp_webapi.DataModels;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Models;

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
                var userId = db.Sprint
                    .Where(x => x.SprintId == sprintReportEditModel.SprintId)
                    .Select(x => x.Owner)
                    .ToString();

                

                sprintReport = db.SprintReport
                    .FirstOrDefault(x => x.SprintId == sprintReportEditModel.SprintId && x.CheckListItemId == sprintReportEditModel.CheckListItemId);

                if (sprintReport == null)
                {
                    throw new KeyNotFoundException("Sprint report does not exist for sprint id: " +
                                                   sprintReportEditModel.SprintId);
                }
                SStatus status = SprintManagementService.GetSprintById(sprintReportEditModel.SprintId).Status;
                if (status != SStatus.Reviewed)
                {
                    sprintReport.ManagerComment = sprintReportEditModel.ManagerComment;
                }
                if (status != SStatus.Closed)
                {
                    sprintReport.UserComment = sprintReportEditModel.UserComment;
                    sprintReport.Status = sprintReportEditModel.Status.ToString();
                    sprintReport.Result = sprintReportEditModel.Result;
                }

                db.SaveChanges();


            }

            return GetSprintReportItemById(sprintReport.SprintReportLineItemId);
        }

        public static string GetSprintreportLineItemIdForCheckListId(string checkListItemId)
        {
            using (var db = new ErpContext())
            {
                SprintReport sprintReport = db.SprintReport
                    .FirstOrDefault(x =>  x.CheckListItemId == checkListItemId);
                if (sprintReport == null)
                {
                    throw new KeyNotFoundException("Sprint line item id does not exist for checklist item id: " +
                                                   checkListItemId);
                }

                return sprintReport.SprintReportLineItemId;

            }
        }
        public static bool ApproveSprintReportLineItems(string sprintReportLineItemId, string approveId)
        {
          // Update here sprint report line item approve code  
          return true;
        }
        
        public static void AddSprintReportLineItem(string sprintId)
        {
            using (var db = new ErpContext())
            {
                List<string> tasks = db.TaskDetail
                    .Where(task => task.SprintId == sprintId)
                    .Select(task => task.TaskId)
                    .ToList();
                
                tasks.ForEach(task =>
                {
                    List<CheckListItemEditModel> checkListItems = CheckListManagementService.GetCheckList(task, "items");

                    checkListItems.ForEach(checkListItem =>
                    {
                        SprintReport sprintReport = new SprintReport()
                        {
                            SprintReportLineItemId = GetNextAvailableId(),
                            SprintId = sprintId,
                            TaskId = task,
                            CheckListItemId = checkListItem.CheckListItemId,
                            Description = checkListItem.Description,
                            ResultType = checkListItem.ResultType,
                            UserComment = checkListItem.UserComment,
                            Approved = "no action",
                            Status = CStatus.notCompleted.ToString(),
                            WorstCase = checkListItem.WorstCase,
                            BestCase = checkListItem.BestCase,
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