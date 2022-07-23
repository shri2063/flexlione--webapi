using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace flexli_erp_webapi.BsonModels
{
    public  enum ETagType {SearchTag, HashTag}
    
    [BsonDiscriminator("tags")]
    public class Tag
    {
            
          // [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
          [BsonRepresentation(BsonType.ObjectId)]
          public String Id { get; set; }

            [BsonElement("description")]
            public string Keyword { get; set; }
            
            [BsonElement("type")]
            public ETagType Type { get; set; }
           
    }
    
   
    
}