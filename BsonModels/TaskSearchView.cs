using MongoDB.Bson.Serialization.Attributes;

namespace m_sort_server.BsonModels
{
    [BsonDiscriminator("tasks")]
    public class TaskSearchView
    {
        
        
            [BsonElement("task_id")]
            public string TaskId { get; set; }

            [BsonElement("owner")]
            public string Owner { get; set; }
            [BsonElement("description")]
            public string Description{ get; set; } 
        
    }
}