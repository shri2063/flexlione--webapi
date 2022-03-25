using System.Collections.Generic;
using System.Linq;
using m_sort_server.BsonModels;
using m_sort_server.EditModels;

namespace m_sort_server.Services
{
    public class SearchManagementService
    {
        public static List<TaskSearchView> GetTaskListForSearchQuery(SearchQueryEditModel searchQuery)
        {

            List<TaskSearchView> searchTaskList = new List<TaskSearchView>();
            if (searchQuery.Tag != null)
            {
                searchTaskList = TagManagementService
                    .GetSearchTagResult("description", searchQuery.Tag)
                    .Tasks
                    .ToList();
            }

            if (searchQuery.CreatedBy != null)
            {
                searchQuery.CreatedBy.ForEach(x =>
                {
                    searchTaskList = searchTaskList.Where(y =>
                        y.CreatedBy == x).ToList();
                });
            }
            
            if (searchQuery.AssignedTo != null)
            {
                searchQuery.AssignedTo.ForEach(x =>
                {
                    searchTaskList = searchTaskList.Where(y =>
                        y.AssignedTo == x).ToList();
                });
            }
            
            if (searchQuery.Deadline != null)
            {
                searchTaskList = searchTaskList
                    .Where(x => x.Deadline < searchQuery.Deadline)
                    .ToList();
            }
            
            if (searchQuery.Description != null)
            {
                searchTaskList = searchTaskList
                    .Where(x => x.Description.ToLower()
                        .Contains(searchQuery.Description.ToLower()))
                    .ToList();
            }
            
            if (searchQuery.Status != null)
            {
                searchQuery.Status.ForEach(x =>
                {
                    searchTaskList = searchTaskList.Where(y =>
                        y.Status == x).ToList();
                });
            }
            
            


            return searchTaskList;

        }
    }
}