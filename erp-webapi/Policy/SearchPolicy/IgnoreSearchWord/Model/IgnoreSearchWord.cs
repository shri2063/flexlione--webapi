using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace flexli_erp_webapi.BsonModels
{
    [BsonDiscriminator("ignore-search-words")]
    public class IgnoreSearchWord
    {
        // [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        [BsonRepresentation(BsonType.ObjectId)]
        public String Id { get; set; }

        [BsonElement("word")]
        public string Keyword { get; set; }
    }
}