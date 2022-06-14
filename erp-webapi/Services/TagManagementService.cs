using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using m_sort_server.BsonModels;
using m_sort_server.EditModels;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using S = Swashbuckle.AspNetCore.Swagger.Tag;

namespace m_sort_server.Services
{
    public class TagManagementService
    {


        public static SearchTagResult GetSearchTagResult(string param, string value)
        {
            var collection = GetCollection("search_tag_results");
            
            // Retrieve value
            var filter = Builders<BsonDocument>.Filter.Eq(param.ToLower(), value.ToLower());
            var searchTagDocument = collection.Find(filter).FirstOrDefault().ToBsonDocument();
            if (searchTagDocument == null)
            {
                return null;
            }
            return BsonSerializer.Deserialize<SearchTagResult>(searchTagDocument);
            
        }
        
        public static List<SearchTag> UpdateTagsContainingTask(TaskDetailEditModel taskDetail)
        {
            List<SearchTag> searchTags = GetSearchTagList();
            foreach (var searchTag in searchTags)
            {
                if (CheckIfTaskContainsTag(taskDetail, searchTag.Description))
                {
                    UpdateTaskListOfSearchTagResult(searchTag.Description);
                }
            }

            return searchTags;
        }

       
        
     
        
        // Adds tag in Search_tags
        // Find tasks matching the Search_tag
        // Updates the same in Search_tag_results
        public static SearchTagResult AddOrUpdateTagWithResult(string tag)
        {
            SearchTag newTag;
            if (!CheckIfSearchTagAlreadyAdded(tag))
            {
                 newTag = AddSearchTag(tag);
            }
            else
            {
                newTag = GetSearchTag("description",tag);
            }
            
            
          
            if (GetSearchTagResult("description",tag) == null)
            {
                SearchTagResult searchTagResult = new SearchTagResult()
                {
                    Id = newTag.Id,
                    Description = tag.ToLower(),
                    Tasks = GetTaskListForSearchParameter(tag)
                };  
                var collection = GetCollection("search_tag_results");
                collection.InsertOne(searchTagResult.ToBsonDocument());
            }
            else
            {
                UpdateTaskListOfSearchTagResult(tag);
            }
            

            // Add A document to Search Tag Result
           
            return GetSearchTagResult("description", tag);

        }
        
        // Check description of all tasks matching tag
        // Filters one which are a match
        // And update the same (removes previous one) in Mongo Db
        private static void UpdateTaskListOfSearchTagResult(string search)
        {
            if (!CheckIfSearchTagAlreadyAdded(search))
            {
                throw new KeyNotFoundException("Tag Not  Added");
            }
            var arrayFilter = Builders<BsonDocument>.Filter.Eq("description", search);
            List<TaskSearchView> taskList = GetTaskListForSearchParameter(search);
            if (GetSearchTagResult("description",search) != null)
            {
                var collection = GetCollection("search_tag_results");
                var arrayUpdate = Builders<BsonDocument>.Update.Set("tasks", taskList);
                collection.UpdateOneAsync(arrayFilter , arrayUpdate); 
            }

           
        }

        private static bool CheckIfTaskContainsTag(TaskDetailEditModel taskDetail, string tag)
        {
            if (taskDetail.Description.ToLower().Contains(tag.ToLower()))
            {
                return true;
            }

            return false;
        }

        public static List<SearchTag> GetSearchTagList()
        {
            var collection = GetCollection("search_tags");
            var documents = collection.Find(new BsonDocument()).ToList();
            List<SearchTag> tags = new List<SearchTag>();
            foreach (var document in documents)
            {
                tags.Add(BsonSerializer
                    .Deserialize<SearchTag>(document));
            }

            return tags;

        }


        
        private static SearchTag AddSearchTag(string search)
        {
            if (CheckIfSearchTagAlreadyAdded(search))
            {
                  throw new KeyNotFoundException("Tag Already Added");
            }   
          
            
            SearchTag newTag = new SearchTag()
            {
                Id = new  Random().Next(100000,1000000),
                Description = search.ToLower()
            };
            
            var collection = GetCollection("search_tags");
            var documentNext = newTag.ToBsonDocument();
            collection.InsertOne(documentNext);

            return GetSearchTag("description", search);
        }

        private static bool CheckIfSearchTagAlreadyAdded(string search)
        {
            List<SearchTag> searchTags = GetSearchTagList();

            foreach (var searchTag in searchTags)
            {
                if (searchTag.Description  == search.ToLower())
                {
                    return true;
                } 
            }

            return false;
        }
        
        
        
        private static SearchTag GetSearchTag(string param, string value)
        {
            var collection = GetCollection("search_tags");
            
            // Retrieve value
            var filter = Builders<BsonDocument>.Filter.Eq(param.ToLower(), value.ToLower());
            var searchTagDocument = collection.Find(filter).FirstOrDefault().ToBsonDocument();
            return BsonSerializer.Deserialize<SearchTag>(searchTagDocument);
            
        }


        private static IMongoCollection<BsonDocument> GetCollection(string collectionId)
        {
            MongoClient dbClient = new MongoClient("mongodb+srv://shrikant:Flexli123@freecluster.6nw7e.mongodb.net");
            var database = dbClient.GetDatabase("tags");
            return database.GetCollection<BsonDocument>(collectionId);
        }

        private static List<TaskSearchView> GetTaskListForSearchParameter(string search)
        {
            List<string> taskIds = (from s in TaskManagementService.GetTaskIdList()
                select s.TaskId).ToList();
           List<TaskDetailEditModel> taskList = new List<TaskDetailEditModel>();
           List<TaskSearchView> taskListContainingSearch = new List<TaskSearchView>();
           foreach (var taskId in taskIds)
           {
               taskList.Add(TaskManagementService.GetTaskById(taskId));
           }

           foreach (var task in taskList)
           {
               if (task.Description.ToLower().Contains(search.ToLower()))
               {
                   taskListContainingSearch.Add(new TaskSearchView()
                   {
                       TaskId = task.TaskId,
                       Description = task.Description,
                       CreatedBy = task.CreatedBy,
                       AssignedTo = task.AssignedTo,
                       Deadline = task.Deadline,
                       Status = task.Status.ToString(),
                       IsRemoved = task.IsRemoved
                   });
               }   
           }

           return taskListContainingSearch;

        }
    }
}