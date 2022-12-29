using flexli_erp_webapi.BsonModels;
using MongoDB.Driver;

namespace flexli_erp_webapi.DataLayer.Interface
{
    public interface ITemplateTagContext
    {
        IMongoCollection<TagTemplateList> TemplateTagSearchResult { get; }
        
        IMongoCollection<IgnoreSearchWord> IgnoreSearchWordList { get; }
    }
}