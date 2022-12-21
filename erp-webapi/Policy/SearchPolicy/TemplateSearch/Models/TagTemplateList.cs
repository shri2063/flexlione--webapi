using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace flexli_erp_webapi.BsonModels
{
    [BsonDiscriminator("template-tag-search-result")]
    
    public class TagTemplateList
    {
        // [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("description")]
        public string Keyword { get; set; }
        
        [BsonElement("templates")]
        public IList<TemplateSearchView> Templates{ get; set; }
    }
}