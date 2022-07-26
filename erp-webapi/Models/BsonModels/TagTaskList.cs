using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace flexli_erp_webapi.BsonModels
{
    [BsonDiscriminator("tag-tasklist")]
    public class TagTaskList
    {
        // [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("description")]
        public string Keyword { get; set; }
        
        [BsonElement("type")]
        public ETagType Type { get; set; }
        
        [BsonElement("tasks")]
        public IList<TaskSearchView> Tasks{ get; set; } 
    }
}