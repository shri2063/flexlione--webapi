using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace flexli_erp_webapi.DataModels
{
    [Table("exp_table")]
    public class RegisterTimeStamp
    {
        [Key] [Column("stamp_id")]
        public string StampId { get; set; }

        [Column("stamp")]
        public DateTime? Stamp { get; set; }
        
    }
}