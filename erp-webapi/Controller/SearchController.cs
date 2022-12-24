using System.Collections.Generic;
using System.Threading.Tasks;
using flexli_erp_webapi.BsonModels;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Repository;
using flexli_erp_webapi.Repository.Interfaces;
using flexli_erp_webapi.Services;
using flexli_erp_webapi.Services.Interfaces;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace flexli_erp_webapi.Controller
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [EnableCors("AllowOrigin")]
    [Produces("application/json")]
    
    public class SearchController : ControllerBase
    {
        
        private readonly ITemplateTagSearchResultRepository _templateTagSearchResultRepository;
        private readonly TagSearchManagementService _tagSearchManagementService;
        private readonly ISearchPriorityPolicy _searchPriorityByCommonalityPolicy;
        private readonly SearchByLabelManagementService _searchByLabelManagementService;
        
        public SearchController(
            ITemplateTagSearchResultRepository templateTagSearchResultRepository,
            TagSearchManagementService tagSearchManagementService,
            ISearchPriorityPolicy searchPriorityByCommonalityPolicy,
            SearchByLabelManagementService searchByLabelManagementService)
        {
          
            _templateTagSearchResultRepository = templateTagSearchResultRepository;
           _tagSearchManagementService = tagSearchManagementService;
           _searchPriorityByCommonalityPolicy = searchPriorityByCommonalityPolicy;
           _searchByLabelManagementService = searchByLabelManagementService;
        }
        
        
        // all tags from template-tags
        [HttpGet("GetListForTemplateTags")]
        [Consumes("application/json")]

        public async Task<ActionResult<IEnumerable<TemplateTag>>> GetTagListForTemplates(int? pageIndex = null, int? pageSize = null)
        {
            var tagList= await _templateTagSearchResultRepository.GetListOfTemplateTags();
            return Ok(tagList);
        }



        // Implement search function
        /// <summary>
        /// Enter space separated tags
        /// </summary>
        [HttpPost("GetSearchResultForTemplates")]
        [Consumes("application/json")]
        
        // Note: Here we are using TemplateEditModel Object
        // In case of Tag Search, we will will convert Search model into TaskDetail model
        public ActionResult<List<TemplateEditModel>> GetSearchResultForTemplates(string searchQuery)
        {
            return _tagSearchManagementService.GetTemplateListForSearchQuery(searchQuery);
        }
        
        /// <summary>
        /// include = list of label tags, currently, "sprint","notCompleted"
        /// </summary>
        [HttpGet("SearchByProfileId")]
        [Consumes("application/json")]

        public async Task<List<TaskSearchView>> SearchByProfileId(string profileId, List<string> include = null, int?pageIndex = null, int? pageSize = null)
        {
            return await _searchByLabelManagementService.SearchByProfileId(profileId, include, pageIndex, pageSize);
        }
    }
}