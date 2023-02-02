using System;
using System.Collections.Generic;
using System.Linq;
using flexli_erp_webapi.DataModels;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Repository.Interfaces;
using mflexli_erp_webapi.Repository.Interfaces;

namespace flexli_erp_webapi.Repository
{
    public class SprintReportRepository: ISprintReportRepository

    {
      
        
        
        public  List<SprintReportEditModel> GetSprintReportForSprint(string sprintId, int? pageIndex = null, int? pageSize = null)
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

        
        
       

        public void AddOrUpdateSprintReportLineItem(SprintReportEditModel sprintReportEditModel)
        {
            using (var db = new ErpContext())
            {
              
                    var existingSprintReportLineItem = db.SprintReport
                        .FirstOrDefault(x => x.SprintReportLineItemId == sprintReportEditModel.SprintReportLineItemId);
                if (existingSprintReportLineItem == null)
                {
                    SprintReportLineItem sprintReportLineItem = new SprintReportLineItem()
                    {
                        SprintReportLineItemId = GetNextAvailableId(),
                        SprintId = sprintReportEditModel.SprintId,
                        TaskId = sprintReportEditModel.TaskId,
                        TaskDescription =  sprintReportEditModel.TaskDescription,
                        CheckListItemId = sprintReportEditModel.CheckListItemId,
                        Description = sprintReportEditModel.Description,
                        ResultType = sprintReportEditModel.ResultType.ToString(),
                        UserComment = sprintReportEditModel.UserComment,
                        Approved = Convert.ToString(sprintReportEditModel.Approved),
                        Status =  Convert.ToString(sprintReportEditModel.Status),
                        WorstCase = sprintReportEditModel.WorstCase,
                        BestCase = sprintReportEditModel.BestCase,
                        Score = 0
                    };
                    db.SprintReport.Add(sprintReportLineItem);
                    db.SaveChanges();
                }
                else
                {
                    existingSprintReportLineItem.Result = sprintReportEditModel.Result;
                    existingSprintReportLineItem.UserComment = sprintReportEditModel.UserComment;
                    existingSprintReportLineItem.Status = sprintReportEditModel.Status.ToString();

                    db.SaveChanges();
                }
               
            }
        }
        
       
        public  SprintReportEditModel GetSprintReportItemById(string sprintReportLineItemId)
        {
            SprintReportLineItem sprintReportLineItem;
            using (var db = new ErpContext())
            {
                sprintReportLineItem = db.SprintReport
                    .FirstOrDefault(x => x.SprintReportLineItemId == sprintReportLineItemId);

                if (sprintReportLineItem == null)
                {
                    return null;
                }

                SprintReportEditModel sprintReportEditModel = new SprintReportEditModel()
                {
                    SprintReportLineItemId = sprintReportLineItem.SprintReportLineItemId,
                    TaskId = sprintReportLineItem.TaskId,
                    TaskDescription = sprintReportLineItem.TaskDescription,
                    CheckListItemId = sprintReportLineItem.CheckListItemId,
                    Description = sprintReportLineItem.Description,
                    ResultType = (CResultType) Enum.Parse(typeof(CResultType), sprintReportLineItem.ResultType,true),
                    Result = sprintReportLineItem.Result,
                    UserComment = sprintReportLineItem.UserComment,
                    ManagerComment = sprintReportLineItem.ManagerComment,
                    Approved = (SApproved) Enum.Parse(typeof(SApproved),sprintReportLineItem.Approved,true),
                    Status = (CStatus) Enum.Parse(typeof(CStatus), sprintReportLineItem.Status, true),
                    WorstCase = sprintReportLineItem.WorstCase,
                    BestCase = sprintReportLineItem.BestCase,
                    Score = sprintReportLineItem.Score,
                    SprintId = sprintReportLineItem.SprintId
                };

                return sprintReportEditModel;
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