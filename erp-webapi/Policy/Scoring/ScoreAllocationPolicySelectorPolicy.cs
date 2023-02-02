using System.Collections.Generic;
using System.Linq;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Services.Interfaces;

namespace flexli_erp_webapi.Services
{
    public class ScoreAllocationPolicySelectorPolicy

    {
        private readonly IEnumerable<IScoreAllocationPolicy> _calculateScoreForTaskPolicies;

        
        public ScoreAllocationPolicySelectorPolicy(IEnumerable<IScoreAllocationPolicy> scorePolicies)
        {
            _calculateScoreForTaskPolicies = scorePolicies;
        }

        IScoreAllocationPolicy _scoreAllocationPolicy;

        ///<Summary>
        /// Get the policy & calculate score accordingly
        ///</Summary>
        public decimal CalculateSprintScore(string sprintId, EScorePolicyType policyType)
        {
           
            decimal score = 0;
            
            _scoreAllocationPolicy = _calculateScoreForTaskPolicies
                .FirstOrDefault(x => x.getType().ToString() == policyType.ToString());
                if (_scoreAllocationPolicy != null)
                {
                    score = _scoreAllocationPolicy.calculateScore(sprintId);
                }
                
                return score;
        }

    }
    
}