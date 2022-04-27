using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace m_sort_server.DataModels
{
    [Table("profile")]
    public class Profile
    {
        [Key] [Column("profile_id")] 
        public string ProfileId { get; set; }

        [Column("type")] 
        public string Type { get; set; }

        [Column("name")]
        public string Name { get; set; }
    }
}