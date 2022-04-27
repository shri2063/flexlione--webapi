using System.Collections.Generic;

namespace m_sort_server.EditModels
{
    public class ProfileEditModel
    {
        public string ProfileId { get; set; }

        public string Type { get; set; }
        
        public string Name { get; set; }
        
        public List<SprintEditModel> sprints { get; set; }
        
    }
}