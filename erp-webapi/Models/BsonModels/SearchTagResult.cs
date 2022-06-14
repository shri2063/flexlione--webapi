using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace m_sort_server.BsonModels
{
    public class SearchTagResult
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        public int Id { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }
        [BsonElement("tasks")]
        public IList<TaskSearchView> Tasks{ get; set; } 
    }
}