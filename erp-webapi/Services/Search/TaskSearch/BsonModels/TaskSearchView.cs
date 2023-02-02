using System;
using System.Data;
using MongoDB.Bson.Serialization.Attributes;

namespace flexli_erp_webapi.BsonModels
{
    [BsonDiscriminator("tasks")]
    public class TaskSearchView
    {
        
        
            [BsonElement("task_id")]
            public string TaskId { get; set; }

            [BsonElement("created_by")]
            public string CreatedBy { get; set; }
            
            [BsonElement("description")]
            public string Description{ get; set; }
            
            [BsonElement("assigned_to")]
            public string AssignedTo{ get; set; }
            
            [BsonElement("deadline")]
            public DateTime? Deadline{ get; set; }
            
            [BsonElement("status")]
            public string Status{ get; set; }
            
            [BsonElement("is_removed")]
            public bool? IsRemoved{ get; set; }
            
            [BsonElement("created_at")]
            public DateTime CreatedAt{ get; set; }
            
            [BsonElement("edited_at")]
            public DateTime EditedAt{ get; set; }
        
    }
}