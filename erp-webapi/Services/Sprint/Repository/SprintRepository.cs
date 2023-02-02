using System;
using System.Data;
using System.Linq;
using flexli_erp_webapi.DataModels;
using flexli_erp_webapi.EditModels;
using m_sort_server.Repository.Interfaces;

namespace flexli_erp_webapi.Repository.Interfaces
{
    public class SprintRepository : ISprintRepository
    
    {
       
        
      
        
        public  SprintEditModel AddOrUpdateSprint(SprintEditModel sprintEditModel)
        {
            Sprint sprint;
            
            using (var db = new ErpContext())
            {
                sprint = db.Sprint
                    .FirstOrDefault(x => x.SprintId == sprintEditModel.SprintId);


                if (sprint != null) // update
                {
                    
                    sprint.Description = sprintEditModel.Description;
                    sprint.SprintNo = sprintEditModel.SprintNo;
                    sprint.Owner = sprintEditModel.Owner;
                    sprint.FromDate = sprintEditModel.FromDate;
                    sprint.ToDate = sprintEditModel.ToDate;
                    sprint.Score = sprintEditModel.Score;
                    sprint.ScorePolicy = sprintEditModel.ScorePolicy.ToString();
                    db.SaveChanges();
                }
                else
                {
                    sprint = new Sprint()
                    {
                        SprintId = GetNextAvailableId(),
                        Description = sprintEditModel.Description,
                        Owner = sprintEditModel.Owner,
                        FromDate = sprintEditModel.FromDate,
                        ToDate = sprintEditModel.ToDate,
                        Status = SStatus.Planning.ToString(),
                        ScorePolicy = sprintEditModel.ScorePolicy.ToString(),
                        Score = 0,
                        Approved = false,
                        Closed = false
                    };
                    db.Sprint.Add(sprint);
                    db.SaveChanges();
                }
            }

            return GetSprintById(sprint.SprintId);
        }
        
        public  void DeleteSprint(string sprintId)
        {
            using (var db = new ErpContext())
            {
                // Get Selected Profile
                Sprint existingSprint = db.Sprint
                    .FirstOrDefault(x => x.SprintId == sprintId);
                
                // [Check]: Sprint is in planning state 
                if (existingSprint != null)
                {
                    if (existingSprint.Status != SStatus.Planning.ToString())
                    {
                        throw new ConstraintException("Cannot delete the sprint, status is not planning");
                    }
                    
                    db.Sprint.Remove(existingSprint);
                    db.SaveChanges();
                    
                }


            }
        }

        public SprintEditModel UpdateSprintStatus(string sprintId, string status)
        {
            using (var db = new ErpContext())
            {
                var sprint = db.Sprint
                    .FirstOrDefault(x => x.SprintId == sprintId);
                sprint.Status = status;
                if (status == "Approved")
                {
                    sprint.Approved = true;
                }
                if (status == "Closed")
                {
                    sprint.Closed = true;
                    sprint.ToDate = DateTime.Today;
                }
                db.SaveChanges();
            }

            return GetSprintById(sprintId);
        }


        public  SprintEditModel GetSprintById(string sprintId)
        {
            SprintEditModel sprint =  GetSprintByIdFromDb(sprintId);
            if (sprint == null)
            {
                return null;
            }

            return sprint;
        }
         
         
         private SprintEditModel GetSprintByIdFromDb (string sprintId)
         {
             using (var db = new ErpContext())
             {
                
                 Sprint existingSprint = db.Sprint
                     .FirstOrDefault(x => x.SprintId == sprintId);
                
                 // [Check]: Sprint does not exist
                 if (existingSprint == null)
                 {
                     return null;
                 }

                 SprintEditModel sprintEditModel = new SprintEditModel()
                 {
                     SprintId = existingSprint.SprintId,
                     Description = existingSprint.Description,
                     Owner = existingSprint.Owner,
                     FromDate = existingSprint.FromDate,
                     ToDate = existingSprint.ToDate,
                     Score = existingSprint.Score,
                     Status = (SStatus) Enum.Parse(typeof(SStatus), existingSprint.Status, true),
                     Approved = existingSprint.Approved,
                     Closed = existingSprint.Closed,
                     SprintNo = existingSprint.SprintNo,
                     ScorePolicy = (EScorePolicyType)Enum.Parse(typeof(EScorePolicyType), existingSprint.ScorePolicy, true)
                     
                 };

                 return sprintEditModel;
             }

         }
         
         private static string GetNextAvailableId()
         {
             using (var db = new ErpContext())
             {
                 var a = db.Sprint
                     .Select(x => Convert.ToInt32(x.SprintId))
                     .DefaultIfEmpty(0)
                     .Max();
                 return Convert.ToString(a + 1);
             }
          
         }
         
       
    }


}