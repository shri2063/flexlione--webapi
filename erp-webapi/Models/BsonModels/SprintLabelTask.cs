using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace flexli_erp_webapi.BsonModels
{
    [BsonDiscriminator("sprint-tasks")]
    
    public class SprintLabelTask
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public String Id { get; set; }
        
        [BsonElement("Task")]
        public TaskSearchView Task { get; set; }
    }
}