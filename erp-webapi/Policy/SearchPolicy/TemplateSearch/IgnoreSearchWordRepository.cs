using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using flexli_erp_webapi.BsonModels;
using flexli_erp_webapi.DataLayer.Interface;
using flexli_erp_webapi.Policy.SearchPolicy.TemplateSearch.Interfaces;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace flexli_erp_webapi.Policy.SearchPolicy.TemplateSearch
{
    public class IgnoreSearchWordRepository : IIgnoreSearchWordRepository
    {
        private readonly ITemplateTagContext _templateTagContext;

        public IgnoreSearchWordRepository(ITemplateTagContext templateTagContext)
        {
            _templateTagContext = templateTagContext;
        }

        public async Task<List<IgnoreSearchWord>> GetIgnoreSearchWordList()
        {
            return await _templateTagContext
                .IgnoreSearchWordList
                .Find(x=>true)
                .ToListAsync();
        }

        public async Task<IgnoreSearchWord> AddIgnoreSearchWordToDb(string keyword)
        {
            // create new object
            IgnoreSearchWord newWord = new IgnoreSearchWord()
            {
                Keyword = keyword.ToLower()
            };

            // Check if already exist
            var existingWord = _templateTagContext
                .IgnoreSearchWordList
                .Find(x=>x.Keyword == newWord.Keyword)
                .FirstOrDefault();

            if (existingWord != null)
            {
                return existingWord;
            }
            
            // Creating new word document and adding
            await _templateTagContext.IgnoreSearchWordList.InsertOneAsync(newWord);

            var createdWord = _templateTagContext
                .IgnoreSearchWordList
                .Find(x=>x.Keyword == newWord.Keyword)
                .FirstOrDefault();

            if (createdWord == null)
            {
                throw new KeyNotFoundException("Tag not created");
            }

            return createdWord;
        }

        public async Task<bool> DeleteIgnoreSearchWordFromDb(string keyword)
        {
            FilterDefinition<IgnoreSearchWord> filterForWord = Builders<IgnoreSearchWord>.Filter.Eq(p => p.Keyword, keyword.ToLower());
            
            DeleteResult deleteTag = await _templateTagContext
                    .IgnoreSearchWordList
                    .DeleteOneAsync(filterForWord);
            
            return deleteTag.IsAcknowledged;
        }
    }
}