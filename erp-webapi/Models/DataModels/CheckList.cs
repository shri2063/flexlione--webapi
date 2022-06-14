using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace m_sort_server.DataModels
{
   
    [Table("check_list")]
    public class CheckList
    {
       [Key][Column("id")]
        public string CheckListItemId { get; set; }
        
        [Column("task_id")]
        public string TaskId { get; set; }
        
        [Column("description")]
        public string Description { get; set; }
        
        [Column("status")]
        public string Status { get; set; }
        
        [Column("comment")]
        public string Comment { get; set; }
        
        [Column("attachment")]
        public string Attachment { get; set; }
 }
}