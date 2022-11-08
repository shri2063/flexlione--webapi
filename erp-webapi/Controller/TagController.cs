using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Threading.Tasks;
using flexli_erp_webapi.BsonModels;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Services;
using flexli_erp_webapi.DataModels;
using flexli_erp_webapi.Repository;
using flexli_erp_webapi.Repository.Interfaces;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Tag = flexli_erp_webapi.BsonModels.Tag;

namespace flexli_erp_webapi.Controller
{
    
    [Route("api/v1/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [EnableCors("AllowOrigin")]
    [Produces("application/json")]

    public class TagController : ControllerBase
    {
        private readonly ITagRepository _tagRepository;
        private readonly ITagTaskListRepository _tagTaskListRepository;
        private readonly SearchManagementService _searchManagementService;
       
       // [ToDo] We need to create a separate tagmanagement Service (Business layer) with which Controller interacts.
       // [ToDo] repository (DB layer) ideally should be directly injected into controller
        public TagController(ITagRepository tagRepository, ITagTaskListRepository tagTaskListRepository, SearchManagementService searchManagementService)
        {
            _tagRepository = tagRepository;
            _tagTaskListRepository = tagTaskListRepository;
           _searchManagementService = searchManagementService;
        }
        
        [HttpGet("GetTagList")]
        [Consumes("application/json")]

        public async Task<ActionResult<IEnumerable<Tag>>> GetTagList(ETagType tagType, int? pageIndex = null, int? pageSize = null)
        {
            var tagList= await _tagRepository.GetSearchTagList(tagType, pageIndex, pageSize);
            return Ok(tagList);
        }
        
        [HttpGet("GetTag")]
        [Consumes("application/json")]
        

        public async Task<ActionResult<IEnumerable<Tag>>> GetTag(string tag, TagRepository.ESearchType searchType, ETagType tagType, int? pageIndex = null, int? pageSize = null)
        {
           
            return Ok(  await _tagRepository.GetSearchTag(tag, searchType, tagType, pageIndex, pageSize));
        }
        
        [HttpGet("GetTaskListForTag")]
        [Consumes("application/json")]

        public Task<TagTaskList> GetTaskListForTag(string search, ETagType tagType)
        {
            return _tagTaskListRepository.GetTagTaskListForTag(search, tagType);
        }
        /// <summary>
        /// tagType values ("SearchType", "HashType")
        /// </summary>
        /// <returns></returns>
        [HttpPut("AddTag")]
        [Consumes("application/json")]

        public async Task<IEnumerable<Tag>> AddTag(string tag, ETagType tagType)
        {
            return await _tagRepository.CreateSearchTag(tag, tagType);
        }
        [HttpPut("ReviseTaskListForSearchTag")]
        [Consumes("application/json")]

        public async Task<TagTaskList> ReviseTaskListForSearchTag(string tag)
        {
            return await _tagTaskListRepository.ReviseTaskListForSearchTag(tag);
        }
        
        [HttpPut("AddTaskToTagTask")]
        [Consumes("application/json")]

        public async Task<TagTaskList> AddTaskToTagTask(string tag, List<string> taskIdList, ETagType tagType)
        {
            return await _tagTaskListRepository.AddTaskToTagTask(tag, taskIdList, tagType);
        }
        
        
        
        
        
        [HttpDelete("DeleteTag")]
        [Consumes("application/json")]

        public Task<bool> DeleteTag(string tag, ETagType type)
        {
            return _tagRepository.DeleteSearchTag(tag, type);
        }
        
    
        [HttpDelete("RemoveTaskFromTagTask")]
        [Consumes("application/json")]

        public Task<TagTaskList> RemoveTaskFromTagTask(string tag, ETagType tagType,string taskId = null)
        {
            return _tagTaskListRepository.RemoveSearchTaskListFromTag(tag, tagType, taskId);
        }
        
        
        
        
        
        [HttpPost("GetSearchResult")]
        [Consumes("application/json")]

        // Note: Here we are using TaskDetailModel Object for both Tag vs Detail Search
        // In case of Tag Search, we will will convert Search model into TaskDetail model
        public ActionResult<List<TaskDetailEditModel>> GetSearchResult(SearchQueryEditModel searchQuery)
        {
            return _searchManagementService.GetTaskListForSearchQuery(searchQuery);
        }
    }

}