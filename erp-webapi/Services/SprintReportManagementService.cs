using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using flexli_erp_webapi.DataModels;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Repository.Interfaces;
using mflexli_erp_webapi.Repository.Interfaces;

namespace flexli_erp_webapi.Services
{
    public class SprintReportManagementService
    {

        private readonly ISprintRepository _sprintRepository;
        private readonly ICheckListRepository _checkListRepository;
        private readonly ISprintReportRepository _sprintReportRepository;
        public SprintReportManagementService(ISprintRepository sprintRepository, ICheckListRepository checkListRepository, ISprintReportRepository sprintReportRepository)
        {
            _sprintRepository = sprintRepository;
            _checkListRepository = checkListRepository;
            _sprintReportRepository = sprintReportRepository;
        }
        
        

       
        public  SprintReportEditModel ReviewCheckList(SprintReportEditModel sprintReportEditModel, string approverId)
        {
            // If checklist exist - get Sprint Status from DB
            var sprintReportLineItem = _sprintReportRepository.GetSprintReportItemById(sprintReportEditModel.SprintReportLineItemId);
            if (sprintReportLineItem == null)
            {
                throw new KeyNotFoundException("Sprint report does not exist" + sprintReportEditModel.SprintReportLineItemId);
            }

            var sprint = _sprintRepository.GetSprintById(sprintReportLineItem.SprintId);
            
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

            return _sprintReportRepository.GetSprintReportItemById(sprintReportEditModel.SprintReportLineItemId);
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