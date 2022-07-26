using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using flexli_erp_webapi.BsonModels;

namespace flexli_erp_webapi.Repository.Interfaces
{
    public interface ITagRepository
    {
        Task<IEnumerable<Tag>> GetSearchTagList(ETagType tagType);

        Task<IEnumerable<Tag>>GetSearchTag(string keyword, TagRepository.ESearchType searchType, ETagType tagType);
        

        Task<IEnumerable<Tag>> CreateSearchTag(string keyword, ETagType tagType);

        Task<bool> DeleteSearchTag(string keyword, ETagType tagType);

       
        
        
        
        


    }
}