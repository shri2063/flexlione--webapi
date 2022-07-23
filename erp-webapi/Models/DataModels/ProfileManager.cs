using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace flexli_erp_webapi.DataModels
{
    [Table("profile_manager")]
    public class ProfileManager
    {
        [Key] [Column("id")] 
        public string Id { get; set; }

        [Column("user_id")] 
        public string UserId { get; set; }

        [Column("manager_id")]
        public string ManagerId { get; set; }
    }
}