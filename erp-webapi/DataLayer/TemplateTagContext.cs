using flexli_erp_webapi.BsonModels;
using flexli_erp_webapi.DataLayer.Interface;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace flexli_erp_webapi.DataLayer
{
    public class TemplateTagContext : ITemplateTagContext
    {
        public TemplateTagContext(IConfiguration configuration)
        {
            var client = new MongoClient(configuration.GetValue<string>("MongoDbSetting:ConnectionString"));
            var database = client.GetDatabase(configuration.GetValue<string>("MongoDbSetting:DatabaseName"));
            TemplateTagSearchResult = database.GetCollection<TagTemplateList>("template-tag-search-result");
            IgnoreSearchWordList = database.GetCollection<IgnoreSearchWord>("ignore-search-words");

        }
        public IMongoCollection<TagTemplateList> TemplateTagSearchResult { get; set; }
        
        public IMongoCollection<IgnoreSearchWord> IgnoreSearchWordList { get; set; }
    }
}