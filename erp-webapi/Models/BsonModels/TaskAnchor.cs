using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace flexli_erp_webapi.BsonModels
{
    [BsonDiscriminator("task-anchor")]
    
    public class TaskAnchor
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public String Id { get; set; }
        
        [BsonElement("TaskId")]
        public String TaskId { get; set; }
        
        [BsonElement("ChildTaskOrder")]
        public List<String> ChildTaskOrder { get; set; }
        
        [BsonElement("searchResults")]
        public List<String> SearchResults { get; set; }
        
        [BsonElement("Label")]
        public List<String> Label { get; set; }
    }
}