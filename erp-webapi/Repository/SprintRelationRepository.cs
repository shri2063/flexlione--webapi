using System;
using System.Collections.Generic;
using System.Linq;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Services.Interfaces;

namespace flexli_erp_webapi.Repository.Interfaces
{
    public class SprintRelationRepository: ISprintRelationRepository
    {

        private readonly ISprintRepository _sprintRepository;
        public SprintRelationRepository(ISprintRepository sprintRepository)
        {
            _sprintRepository = sprintRepository;
        }
        
        public List<SprintEditModel> GetSprintsForProfileId(string profileId, int? pageIndex = null, int? pageSize = null)
        {
            List<string> sprintIds = GetSprintIdsForProfileId(profileId, pageIndex, pageSize);
            List<SprintEditModel> sprints = new List<SprintEditModel>();
            
            // [Check] At-least one Sprint exists
            if (sprintIds == null)
            {
                return null;
            }

            sprintIds.ForEach(x =>
            {

                sprints.Add(_sprintRepository.GetSprintById(x)); 

            });

            return sprints;
        }
        
       

        
        
        public List<string> GetSprintIdsForProfileId(string profileId, int? pageIndex = null, int? pageSize = null)
        {
            List<string> sprintIds = new List<string>();
            using (var db = new ErpContext())
            {
                // [Check] : Pagination
                if (pageIndex != null && pageSize != null)
                {
                    if (pageIndex <= 0 || pageSize <= 0)
                        throw new ArgumentException("Incorrect value for pageIndex or pageSize");
                
                    // skip take logic
                    sprintIds = db.Sprint
                        .Where(x => x.Owner == profileId)
                        .Select(x => x.SprintId).AsEnumerable()
                        .OrderByDescending(Convert.ToInt32)
                        .Skip(((int) pageIndex - 1) * (int) pageSize).Take((int) pageSize).ToList();

                    if (sprintIds.Count == 0)
                    {
                        throw new ArgumentException("Incorrect value for pageIndex or pageSize");
                    }
                    return sprintIds;
                }
                             
                sprintIds = db.Sprint
                    .Where(x => x.Owner == profileId)
                    .Select(x => x.SprintId).AsEnumerable()
                    .OrderByDescending(Convert.ToInt32)
                    .ToList();
            }

            return sprintIds;
        }

        
    }
}