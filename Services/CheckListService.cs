using System;
using System.Collections.Generic;
using System.Linq;
using m_sort_server.DataModels;
using m_sort_server.EditModels;


namespace m_sort_server.Services
{
    public class CheckListService
    {
        public static List<CheckListItemEditModel> GetCheckList(string taskId, string include)
        {
            List<CheckListItemEditModel> checkListEditView = new List<CheckListItemEditModel>();
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

        public static void DeleteCheckListItem(string checkListId)
        {
            using (var db = new ErpContext())
            {
                

                // Get Selected TasK
                CheckList existingCheckList = db.CheckList
                    .FirstOrDefault(x => x.CheckListItemId == checkListId);
                



                if (existingCheckList != null)
                {

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
                    .FirstOrDefault(x => x.CheckListItemId == checkListItemEditModel.CheckListId);


                if (checkList != null) // update
                {

                    checkList.CheckListItemId = checkListItemEditModel.CheckListId;
                    checkList.Description = checkListItemEditModel.Description;
                    checkList.Status = checkListItemEditModel.Status;
                    checkList.TaskId = checkListItemEditModel.TaskId;
                    checkList.Comment = checkListItemEditModel.Comment;
                    checkList.Attachment = checkListItemEditModel.Attachment;
                    
                    db.SaveChanges();
                }
                else
                {
                    checkList = new CheckList
                    {
                        CheckListItemId = GetNextAvailableId(),
                        Description = checkListItemEditModel.Description,
                        Status = checkListItemEditModel.Status,
                        TaskId = checkListItemEditModel.TaskId,
                        Comment = checkListItemEditModel.Comment,
                        Attachment = checkListItemEditModel.Attachment
                    };
                    db.CheckList.Add(checkList);
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
        
        private static CheckListItemEditModel GetCheckListById(string checkListId)
        {
            using (var db = new ErpContext())
            {
                
                CheckList existingCheckList = db.CheckList
                    .FirstOrDefault(x => x.CheckListItemId == checkListId);
                if (existingCheckList == null)
                    return null;
                CheckListItemEditModel checkListItemEditModel = new CheckListItemEditModel()
                {
                    CheckListId = existingCheckList.CheckListItemId,
                    TaskId = existingCheckList.TaskId,
                    Description = existingCheckList.Description,
                    Status = existingCheckList.Status,
                    Comment = existingCheckList.Comment,
                    Attachment = existingCheckList.Attachment
                    
                };

                return checkListItemEditModel;
            }

        }


    }
}