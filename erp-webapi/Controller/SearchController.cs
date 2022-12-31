using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using flexli_erp_webapi.BsonModels;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Policy.SearchPolicy.TemplateSearch.Interfaces;
using flexli_erp_webapi.Repository;
using flexli_erp_webapi.Repository.Interfaces;
using flexli_erp_webapi.Services;
using flexli_erp_webapi.Services.Interfaces;
using flexli_erp_webapi.Services.TaskSearch;
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
        private readonly SearchByLabelManagementService _searchByLabelManagementService;
        private readonly ITaskTagSearchResultRepository _taskTagSearchResultRepository;
        private readonly TaskSearchManagementService _taskSearchManagementService;
        private readonly IIgnoreSearchWordRepository _ignoreSearchWordRepository;
        private readonly AutoSearchByTagCompilerService _autoSearchByTagCompilerService;

        public SearchController(
            ITemplateTagSearchResultRepository templateTagSearchResultRepository,
            TagSearchManagementService tagSearchManagementService,
            ITaskTagSearchResultRepository taskTagSearchResultRepository,
            TaskSearchManagementService taskSearchManagementService,
            IIgnoreSearchWordRepository ignoreSearchWordRepository,
            SearchByLabelManagementService searchByLabelManagementService,
            AutoSearchByTagCompilerService autoSearchByTagCompilerService)
        {
            _taskTagSearchResultRepository = taskTagSearchResultRepository;
            _templateTagSearchResultRepository = templateTagSearchResultRepository;
            _tagSearchManagementService = tagSearchManagementService;
            _taskSearchManagementService = taskSearchManagementService;
            _ignoreSearchWordRepository = ignoreSearchWordRepository;
            _templateTagSearchResultRepository = templateTagSearchResultRepository;
           _tagSearchManagementService = tagSearchManagementService;
           _searchByLabelManagementService = searchByLabelManagementService;
           _autoSearchByTagCompilerService = autoSearchByTagCompilerService;
        }
        
        
        // all tags from template-tags
        [HttpGet("PopulatePastSearchresult")]
        [Consumes("application/json")]
        public  string PopulatePreviousSearchResult(EAssignmentType type)
        {
           _autoSearchByTagCompilerService.PopulatePreviousSearchResult(type);
           return "ok";

        }
        
        // all tags from template-tags
        [HttpGet("GetListForTemplateTags")]
        [Consumes("application/json")]
        public async Task<ActionResult<IEnumerable<string>>> GetTagListForTemplates(int? pageIndex = null, int? pageSize = null)
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
        public ActionResult<List<TemplateEditModel>> GetSearchResultForTemplates(string searchQuery,
            int? pageIndex = null, int? pageSize = null)
        {
            return _tagSearchManagementService.GetTemplateListForSearchQuery(searchQuery, pageIndex, pageSize);
        }
        
        [HttpGet("GetListForTaskTags")]
        [Consumes("application/json")]
        public async Task<ActionResult<IEnumerable<string>>> GetTagListForTasks(int? pageIndex = null, int? pageSize = null)
        {
            var tagList = await _taskTagSearchResultRepository.GetListOfTaskTags();
            return Ok(tagList);
        }

        // Implement search function
        /// <summary>
        /// Enter space separated tags
        /// </summary>
        [HttpPost("GetSearchResultForTasks")]
        [Consumes("application/json")]

        // Note: Here we are using TemplateEditModel Object
        // In case of Tag Search, we will will convert Search model into TaskDetail model
        public ActionResult<List<TaskDetailEditModel>> GetSearchResultForTasks(string searchQuery,
            int? pageIndex = null, int? pageSize = null)
        {
            return _taskSearchManagementService.GetTaskListForSearchQuery(searchQuery, pageIndex, pageSize);
        }
        
        /// <summary>
        /// </summary>
        [HttpPut("SearchByQuery")]
        [Consumes("application/json")]

        public  List<TaskDetailEditModel> GetTaskListForSearchQuery(SearchQueryEditModel searchQuery)
        {
            return  _taskSearchManagementService.GetTaskListForSearchQuery(searchQuery);
        }

        [HttpGet("GetIgnoreSearchWordList")]
        [Consumes("application/json")]

        public async Task<ActionResult<IEnumerable<IgnoreSearchWord>>> GetIgnoreSearchWordList()
        {
            var wordList= await _ignoreSearchWordRepository.GetIgnoreSearchWordList();
            return Ok(wordList);
        }
        
        [HttpPut("AddIgnoreSearchWord")]
        [Consumes("application/json")]

        public async Task<IgnoreSearchWord> AddIgnoreSearchWord(string keyword)
        {
            return await _ignoreSearchWordRepository.AddIgnoreSearchWordToDb(keyword);
        }

        [HttpDelete("DeleteIgnoreSearchWord")]
        [Consumes("application/json")]

        public Task<bool> DeleteIgnoreSearchWord(string keyword)
        {
            return _ignoreSearchWordRepository.DeleteIgnoreSearchWordFromDb(keyword);
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