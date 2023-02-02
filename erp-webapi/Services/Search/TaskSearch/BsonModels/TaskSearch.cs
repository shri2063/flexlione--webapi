using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace flexli_erp_webapi.BsonModels
{
    [BsonDiscriminator("task-tag-search-result")]
    public class TaskSearch
    {
        // [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("description")]
        public string Keyword { get; set; }
        
        [BsonElement("type")]
        public int Type { get; set; }
        
        
        
        [BsonElement("tasks")]
        public IList<TaskSearchView> Tasks{ get; set; } 
    }
}