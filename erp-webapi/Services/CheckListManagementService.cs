using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using flexli_erp_webapi.DataModels;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Repository.Interfaces;
using flexli_erp_webapi.Services.Interfaces;
using m_sort_server.Repository.Interfaces;
using mflexli_erp_webapi.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;


namespace flexli_erp_webapi.Services
{
    public class 
        CheckListManagementService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly ISprintRepository _sprintRepository;
        private readonly ICheckListRepository _checkListRepository;
        private readonly ISprintReportRepository _sprintReportRepository;
        private readonly ISprintReportRelationRepository _sprintReportRelationRepository;
        public CheckListManagementService(ITaskRepository taskRepository, ISprintRepository sprintRepository
            , ICheckListRepository checkListRepository, ISprintReportRepository sprintReportRepository, 
            ISprintReportRelationRepository sprintReportRelationRepository)
        {
            _taskRepository = taskRepository;
            _sprintRepository = sprintRepository;
            _checkListRepository = checkListRepository;
            _sprintReportRepository = sprintReportRepository;
            _sprintReportRelationRepository = sprintReportRelationRepository;
        }
        
       

        public CheckListItemEditModel CreateOrUpdateCheckListItem(CheckListItemEditModel checkListItemEditModel, string loggedInId)

        {

            if (checkListItemEditModel.AssignmentType == EAssignmentType.Template)
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
                
                return _checkListRepository.CreateOrUpdateCheckListInDb(checkListItemEditModel);
                
                
            }
            
            //[Check]: if checklist description is updating in sprint request for approval stage
            if (SprintStatusCheckForChecklistUpdate (checkListItemEditModel, checkList, sprint != null ?sprint.Status: SStatus.Planning) )
            {
                throw new KeyNotFoundException("Checklist description can't updated after sending sprint for approval");
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
            var updatedCheckList = ApplySprintStatusBasedCheck(checkListItemEditModel, sprint !=null ?sprint.Status: SStatus.Planning);
           
            // can be updated by task owner only
            if (checkListItemEditModel.AssignmentType == EAssignmentType.Task &&  task.AssignedTo != loggedInId )
            {
                throw new Exception("Only task owner can update checklist");
            }  

            
            updatedCheckList = _checkListRepository.CreateOrUpdateCheckListInDb(updatedCheckList);
            _taskRepository.UpdateEditedAtTimeStamp(updatedCheckList.TypeId);
            
             
             //[Action]: Update in Sprint report if Status Approved
             if (sprint != null ? sprint.Approved: false)
             {
                 _sprintReportRepository.AddOrUpdateSprintReportLineItem(GetSprintReportLineItemForCheckListitem(updatedCheckList));
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

        private Boolean SprintStatusCheckForChecklistUpdate(CheckListItemEditModel newChecklist, CheckListItemEditModel oldChecklist, SStatus sprintStatus)
        {
            return ((newChecklist.Description != oldChecklist.Description) 
                    && (sprintStatus != SStatus.Planning)
                    );
        }

        public  void DeleteCheckListItem(string checkListItemId)
        {
            using (var db = new ErpContext())
            {
                

                // Get Selected TasK
                CheckList existingCheckList = db.CheckList
                    .FirstOrDefault(x => x.CheckListItemId == checkListItemId);
                



                if (existingCheckList != null && existingCheckList.CheckListType  == EAssignmentType.Task.ToString())
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
                _sprintReportRelationRepository.GetSprintReportLineItemIdForCheckListId(checkListItem.CheckListItemId);
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