using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using flexli_erp_webapi.BsonModels;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Repository.Interfaces;

namespace flexli_erp_webapi.Services
{
    public class SearchByLabelManagementService
    {
        private readonly ILabelRepository _labelRepository;
        private readonly ITagTaskListRepository _tagTaskListRepository;

        public SearchByLabelManagementService(ILabelRepository labelRepository, ITagTaskListRepository tagtaskListRepository)
        {
            _labelRepository = labelRepository;
            _tagTaskListRepository = tagtaskListRepository;
        }

        public async Task<List<TaskSearchView>> SearchByProfileId(string profileId, List<string> include = null, int? pageIndex = null,
            int? pageSize = null)
        {
            if (include != null)
            {
                if (include.Contains("sprint"))
                {
                    // all sprint tasks for profileId
                    var sprintTask = await _labelRepository.SprintLabelTaskForProfileId(profileId);
                    
                    // if notCompleted also present then filter those who aren't complete
                    if (include.Contains("notCompleted"))
                    {
                        // Pagination
                        if (pageSize != null && pageIndex != null)
                        {
                            return PageOfTask(sprintTask.FindAll(x => x.Status != EStatus.completed.ToString()), pageIndex, pageSize);
                        }
                        
                        return sprintTask.FindAll(x => x.Status != EStatus.completed.ToString());
                    }
                    
                    // return only sprint task
                    // Pagination
                    if (pageSize != null && pageIndex != null)
                    {
                        return PageOfTask(sprintTask, pageIndex, pageSize);
                    }
                    return sprintTask;
                }

                // if only include = notCompleted
                if (include.Contains("notCompleted"))
                {
                    // Pagination
                    if (pageSize != null && pageIndex != null)
                    {
                        return PageOfTask(await _labelRepository.notCompleteLabelTaskForProfileId(profileId), pageIndex, pageSize);
                    }
                    
                    return await _labelRepository.notCompleteLabelTaskForProfileId(profileId);
                }
            }
            
            // include null then empty list return
            return new List<TaskSearchView>();
        }

        private List<TaskSearchView> PageOfTask(List<TaskSearchView> taskList, int? pageIndex, int? pageSize)
        {
            if (pageIndex <= 0 || pageSize <= 0)
                throw new ArgumentException("Incorrect value for pageIndex or pageSize");
            
            // skip take logic
            var pageTaskList = taskList
                .OrderByDescending(t=>t.EditedAt)
                .Skip((int)((pageIndex - 1) * pageSize))
                .Take((int)pageSize)
                .ToList();
            
            if (pageTaskList.Count == 0)
            {
                throw new ArgumentException("Incorrect value for pageIndex or pageSize");
            }
            
            return pageTaskList;
        }
    }
}