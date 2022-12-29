using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using flexli_erp_webapi.DataModels;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Repository.Interfaces;
using mflexli_erp_webapi.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;


namespace flexli_erp_webapi.Services
{
    public class CheckListManagementService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly ISprintRepository _sprintRepository;
        private readonly ICheckListRepository _checkListRepository;
        private readonly ISprintReportRepository _sprintReportRepository;
        public CheckListManagementService(ITaskRepository taskRepository, ISprintRepository sprintRepository
            , ICheckListRepository checkListRepository, ISprintReportRepository sprintReportRepository)
        {
            _taskRepository = taskRepository;
            _sprintRepository = sprintRepository;
            _checkListRepository = checkListRepository;
            _sprintReportRepository = sprintReportRepository;
        }
        
       

        public CheckListItemEditModel CreateOrUpdateCheckListItem(CheckListItemEditModel checkListItemEditModel)
        {

            if (checkListItemEditModel.CheckListType == ECheckListType.Template)
            {
                return _checkListRepository.CreateOrUpdateCheckListInDb(checkListItemEditModel);
            }
            // If checklist exist - get Sprint Status from DB
            var checkList = _checkListRepository.GetCheckListById(checkListItemEditModel.CheckListItemId);
            var task =  (checkList != null)?  _taskRepository.GetTaskById(checkList.TypeId): null;
            var sprint = (task != null) ?_sprintRepository.GetSprintById(
                task.SprintId): null;
            
            // [check] Checklist can added only if sprint is in planning stage
            // If checklist does not exist - get Sprint Status from checklist param in function
            if (checkList == null)
            {
                task = (checkListItemEditModel.TypeId != null)?_taskRepository.GetTaskById(checkListItemEditModel.TypeId)
                    : throw new KeyNotFoundException("Task Id not mentioned");
                sprint = (task != null) ?_sprintRepository.GetSprintById(
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
           
        
            
            updatedCheckList = _checkListRepository.CreateOrUpdateCheckListInDb(updatedCheckList);
             
             //[Action]: Update in Sprint report if Status Approved
             if (sprint != null ? sprint.Approved: false)
             {
                 _sprintReportRepository.UpdateSprintReportLineItem(GetSprintReportLineItemForCheckListitem(updatedCheckList));
             }

             return updatedCheckList;
        }

        private  CheckListItemEditModel ApplySprintStatusBasedCheck(CheckListItemEditModel checkListItemEditModel, SStatus sprintStatus)
        {
            var closureStatus = new List<string> { SStatus.Closed.ToString(), SStatus.RequestForClosure.ToString() };
            CheckListItemEditModel editCheckList = _checkListRepository.GetCheckListById(checkListItemEditModel.CheckListItemId);
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

       

        public  void DeleteCheckListItem(string checkListItemId)
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
                        if (_sprintRepository.GetSprintById(sprintId).Status != SStatus.Planning)
                        {
                            throw new ConstraintException("Checklist cannot be deleted, sprint is already approved");
                        }  
                    }
                   
                    
                    
                }
                
                db.CheckList.Remove(existingCheckList);
                db.SaveChanges();


            }
        }

        
      

        
       

        
        public  SprintReportEditModel GetSprintReportLineItemForCheckListitem(CheckListItemEditModel checkListItem)
        {
            string sprintReportLineItemId =
                _sprintReportRepository.GetSprintreportLineItemIdForCheckListId(checkListItem.CheckListItemId);
            if (sprintReportLineItemId == null)
            {
                throw new KeyNotFoundException("Sprint report lineitem does not exist for checklist item: " +
                                               checkListItem.CheckListItemId);
            }
            SprintReportEditModel sprintReportEditModel = _sprintReportRepository
                .GetSprintReportItemById(sprintReportLineItemId);
            sprintReportEditModel.ManagerComment = checkListItem.ManagerComment;
            sprintReportEditModel.Result = checkListItem.Result;
            sprintReportEditModel.UserComment = checkListItem.UserComment;
            sprintReportEditModel.Status = checkListItem.Status;
            return sprintReportEditModel;
        
        }
        


      
       
        
        

        

        // Create dummy new checklist item for task with no checklist items
       
    }
}