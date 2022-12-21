using System.Collections.Generic;
using System.Threading.Tasks;
using flexli_erp_webapi.BsonModels;
using flexli_erp_webapi.Policy.SearchPolicy.TemplateSearch.Interfaces;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace flexli_erp_webapi.Controller
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [EnableCors("AllowOrigin")]
    [Produces("application/json")]
    
    public class IgnoreSearchWordConroller : ControllerBase
    {
        private readonly IIgnoreSearchWordRepository _ignoreSearchWordRepository;

        public IgnoreSearchWordConroller(IIgnoreSearchWordRepository ignoreSearchWordRepository)
        {
            _ignoreSearchWordRepository = ignoreSearchWordRepository;
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
        
        [HttpDelete("DeleteTag")]
        [Consumes("application/json")]

        public Task<bool> DeleteIgnoreSearchWord(string keyword)
        {
            return _ignoreSearchWordRepository.DeleteIgnoreSearchWordFromDb(keyword);
        }

        
    }
}