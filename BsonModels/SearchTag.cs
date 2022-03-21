﻿using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace m_sort_server.BsonModels
{
    [BsonDiscriminator("search_tags")]
    public class SearchTag
    {
            [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
            public string Id { get; set; }

            [BsonElement("description")]
            public string Description { get; set; }
            [BsonElement("tasks")]
            public IList<TaskSearchView> Tasks{ get; set; } 
        }
    
}