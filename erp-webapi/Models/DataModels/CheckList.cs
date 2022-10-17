using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace flexli_erp_webapi.DataModels
{
    
    [Table("check_list")]
    public class CheckList
    {
       [Key][Column("id")]
        public string CheckListItemId { get; set; }
        
        [Column("type_id")]
        public string TaskId { get; set; }
        
        [Column("description")]
        public string Description { get; set; }
        
        [Column("status")]
        public string Status { get; set; }
        

        [Column("worst_case")]
        public int? WorstCase { get; set; }
        
        [Column("best_case")]
        public int? BestCase { get; set; }
        
        [Column("result_type")]
        public string ResultType { get; set; }
        
        [Column("result")]
        public string Result { get; set; }
        
        [Column("user_comment")]
        public string UserComment { get; set; }
        
        [Column("manager_comment")]
        public string ManagerComment { get; set; }
        
        [Column("essential")]
        public bool Essential { get; set; }
 }
}