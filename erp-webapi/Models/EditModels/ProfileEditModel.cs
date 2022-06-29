using System.Collections.Generic;

namespace flexli_erp_webapi.EditModels
{
    public class ProfileEditModel
    {
        public string ProfileId { get; set; }

        public string Type { get; set; }
        
        public string Name { get; set; }
        
        public string EmailId { get; set; }
        
        public string Password { get; set; }
        
        public List<SprintEditModel> Sprints { get; set; }
        
    }
}