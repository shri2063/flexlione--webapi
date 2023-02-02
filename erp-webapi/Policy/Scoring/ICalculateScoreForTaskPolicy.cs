using System.Collections.Generic;
using System.Linq;
using flexli_erp_webapi.EditModels;

namespace flexli_erp_webapi.Services.Interfaces
{
    public interface IScoreAllocationPolicy 

    {
        decimal calculateScore(string sprintId);

        EScorePolicyType getType();

        
    }
}