using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace flexli_erp_webapi.DataModels
{
    [Table("sprint")]
    public class Sprint
    {
        [Key] [Column("sprint_id")] 
        public string SprintId { get; set; }

        [Column("description")] 
        public string Description { get; set; }

        [Column("owner")]
        public string Owner { get; set; }
        
        [Column("from_date")]
        public DateTime FromDate { get; set; }
        
        [Column("to_date")]
        public DateTime ToDate { get; set; }
        
        [Column("score")]
        public int Score { get; set; }
        
        [Column("deliverable")]
        public string Deliverable { get; set; }
        
        [Column("delivered")]
        public string Delivered { get; set; }
        
       
        
    }
}