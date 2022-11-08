using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using flexli_erp_webapi.BsonModels;
using flexli_erp_webapi.DataLayer.Interface;
using flexli_erp_webapi.Repository.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Tag = flexli_erp_webapi.BsonModels.Tag;

namespace flexli_erp_webapi.Repository
{
    public class TagRepository: ITagRepository
    {

        private readonly ITagContext _tagContext;
        private readonly ITagTaskListRepository _tagTaskListRepository;
        public enum ESearchType
        {
            strong,
            weak
        }
        public TagRepository(ITagContext tagContext, ITagTaskListRepository tagTaskListRepository)
        {
            _tagContext = tagContext ?? throw new ArgumentNullException(nameof(tagContext));
            _tagTaskListRepository = tagTaskListRepository;
        }
        public async Task<IEnumerable<Tag>> GetSearchTagList(ETagType tagType, int? pageIndex = null, int? pageSize = null)
        {

            var tagList = await _tagContext
                .Tag
                .Find(x => x.Type == tagType)
                .ToListAsync();
            
            //[Check]: Pagination check
            if (pageIndex != null && pageSize != null)
            {
                // pageIndex, pageSize must be only positive integers
                if (pageIndex <= 0 || pageSize <= 0)
                    throw new ArgumentException("Incorrect value for pageIndex or pageSize");
                
                // skip limit logic
                var filteredList = tagList
                    .OrderByDescending(x => x.Id)
                    .Skip(((int)pageIndex - 1) * (int)pageSize)
                    .Take((int)pageSize)
                    .ToList();

                if (filteredList.Count == 0)
                {
                    throw new ArgumentException("Incorrect value for pageIndex or pageSize");
                }

                return filteredList;
            }

            return tagList;

        }

        public async Task<IEnumerable<Tag>>GetSearchTag(string searchTag, ESearchType searchType , ETagType tagType, int? pageIndex = null, int? pageSize = null)
        {

            try
            { 
                // [Check]: Pagination Check
                if (pageIndex != null && pageSize != null)
                {
                    if (pageIndex <= 0 || pageSize <= 0)
                        throw new ArgumentException("Incorrect value for pageIndex or pageSize");

                    List<Tag> tagList;

                    // skip limit logic
                    switch (searchType)
                    {
                        case ESearchType.weak:
                            tagList = await _tagContext
                                .Tag
                                .Find(x => x.Keyword.Contains(searchTag.ToLower())
                                           && x.Type == tagType)
                                .ToListAsync();
                            
                            var filteredList = tagList
                                .OrderByDescending(x => x.Id)
                                .Skip(((int)pageIndex - 1) * (int)pageSize)
                                .Take((int)pageSize)
                                .ToList();

                            if (filteredList.Count == 0)
                            {
                                throw new ArgumentException("Incorrect value for pageIndex or pageSize");
                            }

                            return filteredList;
                    }

                    switch (searchType) { case ESearchType.strong: tagList = await _tagContext
                        .Tag
                        .Find(x => x.Keyword == searchTag.ToLower()
                                   && x.Type == tagType)
                        .ToListAsync(); 
                        
                        var filteredList = tagList
                            .OrderByDescending(x => x.Id)
                            .Skip(((int)pageIndex - 1) * (int)pageSize)
                            .Take((int)pageSize)
                            .ToList();
                        
                        if (filteredList.Count == 0)
                        {
                            throw new ArgumentException("Incorrect value for pageIndex or pageSize");
                        }

                        return filteredList;
                    }
                    
                    throw new KeyNotFoundException("Cannot get result for tag " + searchTag);
                }
                
                switch (searchType) { case ESearchType.weak: return await _tagContext
                        .Tag
                        .Find(x => x.Keyword.Contains(searchTag.ToLower()) 
                                   && x.Type == tagType)
                        .ToListAsync(); }
                    
                    switch (searchType) { case ESearchType.strong: return await _tagContext
                        .Tag
                        .Find(x => x.Keyword == searchTag.ToLower()
                                   && x.Type == tagType)
                        .ToListAsync(); }

                    throw new KeyNotFoundException("Cannot get result for tag " + searchTag);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw new KeyNotFoundException(e.Message);
            }
           
        }

   


        public async Task<IEnumerable<Tag>> CreateSearchTag(string keyword, ETagType tagType)
        {
            Tag newTag = new Tag()
            {
                Keyword = keyword,
                Type = tagType
            };

            // Creating Tag document
            // [dbCheck]: Unique constraint on keyword + tagType
            await _tagContext.Tag.InsertOneAsync(newTag);

            var createdTag = await GetSearchTag(keyword, ESearchType.strong, tagType);

            if (createdTag == null)
            {
                throw new KeyNotFoundException("Tag not created");
            }
            
            // [Assumption]  There will be only one created Tag
            var tagTaskList = await _tagTaskListRepository.CreateTagTaskListForTag(keyword, tagType);

            if (tagTaskList == null)
            {
                await DeleteSearchTag(keyword, tagType);
                throw new KeyNotFoundException("Tag Task List could not be created");
            }

            return createdTag;
        }

        public async Task<bool> DeleteSearchTag(string 
            keyword, ETagType tagType)
        {
           
            FilterDefinition<Tag> filterForTag = Builders<Tag>.Filter.Eq(p => p.Keyword, keyword.ToLower());

            var deleteTaskTagList = await _tagTaskListRepository.DeleteTagTaskList(keyword, tagType);

            if (deleteTaskTagList)
            {
                DeleteResult deleteTag = await _tagContext
                    .Tag
                    .DeleteOneAsync(filterForTag);
                return deleteTag.IsAcknowledged;
            }

            return false;
        }
    }
}