using System;
using System.Collections.Generic;
using System.Linq;
using flexli_erp_webapi.DataModels;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Services;

namespace flexli_erp_webapi.Repository.Interfaces
{
    public class SprintRepository : ISprintRepository
    {
        public SprintEditModel GetSprintById(string sprintId)
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
                    SprintNo = existingSprint.SprintNo
                };

                return sprintEditModel;
            }
        }
    }


}