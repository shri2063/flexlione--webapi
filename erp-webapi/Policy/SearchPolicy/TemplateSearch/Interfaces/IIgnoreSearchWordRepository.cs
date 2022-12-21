using System.Collections.Generic;
using System.Threading.Tasks;
using flexli_erp_webapi.BsonModels;
using Microsoft.AspNetCore.Mvc;

namespace flexli_erp_webapi.Policy.SearchPolicy.TemplateSearch.Interfaces
{
    public interface IIgnoreSearchWordRepository
    {
        Task<List<IgnoreSearchWord>> GetIgnoreSearchWordList();

        Task<IgnoreSearchWord> AddIgnoreSearchWordToDb(string keyword);

        Task<bool> DeleteIgnoreSearchWordFromDb(string keyword);
    }
}