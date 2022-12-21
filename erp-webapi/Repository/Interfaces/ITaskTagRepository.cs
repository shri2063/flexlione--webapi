using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using flexli_erp_webapi.BsonModels;

namespace flexli_erp_webapi.Repository.Interfaces
{
    public interface ITagRepository
    {
        Task<IEnumerable<TaskTag>> GetSearchTagList(ETagType tagType, int? pageIndex = null, int? pageSize = null);

        Task<IEnumerable<TaskTag>>GetSearchTag(string keyword, TagRepository.ESearchType searchType, ETagType tagType, int? pageIndex = null, int? pageSize = null);
        

        Task<IEnumerable<TaskTag>> CreateSearchTag(string keyword, ETagType tagType);

        Task<bool> DeleteSearchTag(string keyword, ETagType tagType);

       
        
        
        
        


    }
}