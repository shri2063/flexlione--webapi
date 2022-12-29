using System.Collections.Generic;
using flexli_erp_webapi.EditModels;

namespace mflexli_erp_webapi.Repository.Interfaces
{
    public interface IDependencyRepository
    {
        List<DependencyEditModel> GetUpstreamDependenciesByTaskId(string taskId, 
            int? pageIndex = null, int? pageSize = null);
        
        List<DependencyEditModel> GetDownstreamDependenciesByTaskId(string taskId, 
            int? pageIndex = null, int? pageSize = null);
    }
}