using flexli_erp_webapi.BsonModels;
using MongoDB.Driver;
using Tag = flexli_erp_webapi.BsonModels.Tag;

namespace flexli_erp_webapi.DataLayer.Interface
{
    public interface ITagContext
    {
         IMongoCollection<Tag> Tag { get; }
        
         IMongoCollection<TagTaskList> TagTaskList { get; }
    }
}