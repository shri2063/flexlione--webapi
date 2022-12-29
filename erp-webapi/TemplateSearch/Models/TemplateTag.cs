using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace flexli_erp_webapi.BsonModels
{
    [BsonDiscriminator("template-tags")]
    public class TemplateTag
    {
        // [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        [BsonRepresentation(BsonType.ObjectId)]
        public String Id { get; set; }

        [BsonElement("description")]
        public string Keyword { get; set; }
    }
}