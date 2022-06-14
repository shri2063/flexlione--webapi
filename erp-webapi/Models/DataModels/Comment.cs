using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace m_sort_server.DataModels
{
    [Table("comment")]
    public class Comment
    {
        [Key] [Column("comment_id")]
        public string CommentId { get; set; }
        
        [Column("message")]
        public string Message { get; set; }
            
        [Column("task_id")]
        public string TaskId { get; set; }
            
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
        
        [Column("created_by")]
        public string CreatedBy { get; set; }
    }
}