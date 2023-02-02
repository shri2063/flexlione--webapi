using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using flexli_erp_webapi.BsonModels;
using flexli_erp_webapi.DataLayer.Interface;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Repository.Interfaces;
using flexli_erp_webapi.Services;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace flexli_erp_webapi.Repository
{
    public class TemplateSearchResultRepository : ITemplateSearchResultRepository
    {
        private readonly ITemplateTagContext _templateTagContext;
        
        public TemplateSearchResultRepository(ITemplateTagContext templateTagContext)
        {
            _templateTagContext = templateTagContext ?? throw new ArgumentNullException(nameof(templateTagContext));
        }
        
      
        public async Task<List<string>> GetListOfTemplateTags()
        {
            return  _templateTagContext
                .TemplateTagSearchResult
                .AsQueryable<TagTemplateList>()
                .Select(x => x.Keyword)
                .ToList();
        }
        
        public async Task<TagTemplateList> GetTemplateListForTag(string keyword)
        {
            // returns templates of tags which contains given keyword
            return  await  _templateTagContext
                .TemplateTagSearchResult
                .Find(x => x.Keyword == keyword.ToLower())
                .FirstOrDefaultAsync();
        }

        public async Task<TagTemplateList> CreateTemplateListForTag(string keyword)
        {
            // new template-tag-search-result object
            TagTemplateList newTemplateList = new TagTemplateList()
            {
                Keyword = keyword,
                Templates = new List<TemplateSearchView>()
            };
            
            // Insertion of created template-tag-search-result object
            await _templateTagContext.TemplateTagSearchResult.InsertOneAsync(newTemplateList);
            return await GetTemplateListForTag(keyword);
        }
       

        public async Task<TagTemplateList> AddTemplateToTemplateListOfTag(string keyword, string templateId)
        {
            TagTemplateList existingTagTemplate = await GetTemplateListForTag(keyword.ToLower());
            
            // [Check]: Tag has already been added in TagTemplate DB
            if (existingTagTemplate == null)
            {
                existingTagTemplate = await CreateTemplateListForTag(keyword);
            }

           
            List<string> existingTemplateIds = (from s in  existingTagTemplate.Templates
                select s.TemplateId).ToList();

            if (!existingTemplateIds.Contains(templateId))
            {
                var update = Builders<TagTemplateList>.Update
                    .Push<TemplateSearchView>(e => e.Templates,GetTemplateSearchViewForTemplate(TemplateManagementService.GetTemplateByIdStatic(templateId)) );
                var filter = Builders<TagTemplateList>.Filter.Eq(e => e.Keyword, keyword.ToLower());

                await _templateTagContext.TemplateTagSearchResult.FindOneAndUpdateAsync(filter, update);

            }
            
            
            return await GetTemplateListForTag(keyword);
        }

        public async Task<TagTemplateList> RemoveTemplateFromTemplateListOfTag(string keyword, string templateId)
        {
        
            var filter = Builders<TagTemplateList>.Filter.Eq(e => e.Keyword, keyword.ToLower());

            var update = Builders<TagTemplateList>.Update
                .PullFilter(e => e.Templates, Builders<TemplateSearchView>.Filter.Where(template => template.TemplateId ==  templateId));

            await _templateTagContext.TemplateTagSearchResult.FindOneAndUpdateAsync(filter, update);

            return await GetTemplateListForTag(keyword);
        }


        private static TemplateSearchView GetTemplateSearchViewForTemplate(TemplateEditModel template)
        {
            return new TemplateSearchView()
            {
                TemplateId = template.TemplateId,
                Description = template.Description,
                Owner = template.Owner,
                CloneTemplateId = template.CloneTemplateId,
                CreatedAt = template.CreatedAt,
                Role = template.Role
            };
        }
    }
}