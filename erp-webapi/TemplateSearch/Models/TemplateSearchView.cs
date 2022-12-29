using System;
using MongoDB.Bson.Serialization.Attributes;

namespace flexli_erp_webapi.BsonModels
{
    [BsonDiscriminator("templates")]
    public class TemplateSearchView
    {
        [BsonElement("template_id")]
        public string TemplateId { get; set; }

        [BsonElement("description")]
        public string Description{ get; set; }
        
        [BsonElement("owner")]
        public string Owner { get; set; }
            
        [BsonElement("clone")]
        public string CloneTemplateId{ get; set; }
            
        [BsonElement("created_at")]
        public DateTime CreatedAt{ get; set; }
            
        [BsonElement("role")]
        public string Role{ get; set; }
    }
}