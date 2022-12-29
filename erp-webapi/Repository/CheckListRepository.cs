using System;
using System.Collections.Generic;
using System.Linq;
using flexli_erp_webapi;
using flexli_erp_webapi.DataModels;
using flexli_erp_webapi.EditModels;

namespace mflexli_erp_webapi.Repository.Interfaces
{
    public class CheckListRepository: ICheckListRepository
    {
        public List<CheckListItemEditModel> GetCheckList(string taskId, ECheckListType type, int? pageIndex = null, int? pageSize = null)
        {
            if (pageIndex != null && pageSize != null)
            {
                return GetCheckListPageForTaskId(taskId, type, (int) pageIndex, (int) pageSize);
            }
            return GetCheckListForTypeId(taskId,type);
        }
        public  CheckListItemEditModel CreateOrUpdateCheckListInDb(CheckListItemEditModel checkListItemEditModel)
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

        private  List<CheckListItemEditModel> GetCheckListPageForTaskId(string taskId, ECheckListType type, int pageIndex, int pageSize)
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
        
        private  List<CheckListItemEditModel> GetCheckListForTypeId(string  typeId, ECheckListType type)
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

        public  CheckListItemEditModel GetCheckListById(string checkListItemId)
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
                    Status = (CStatus)Enum.Parse(typeof(CStatus), existingCheckList.Status, true),
                    WorstCase = existingCheckList.WorstCase,
                    BestCase = existingCheckList.BestCase,
                    ResultType = existingCheckList.ResultType != null
                        ? (CResultType)Enum.Parse(typeof(CResultType), existingCheckList.ResultType, true)
                        : CResultType.File,
                    Result = existingCheckList.Result,
                    Essential = existingCheckList.Essential,
                    UserComment = existingCheckList.UserComment,
                    ManagerComment = existingCheckList.ManagerComment,
                    CheckListType =
                        (ECheckListType)Enum.Parse(typeof(ECheckListType), existingCheckList.CheckListType, true),


                };

                return checkListItemEditModel;
            }
        }
    }
}