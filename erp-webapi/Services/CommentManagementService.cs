using System;
using System.Collections.Generic;
using System.Linq;
using flexli_erp_webapi.DataModels;
using flexli_erp_webapi.EditModels;

namespace flexli_erp_webapi.Services
{
    public class CommentManagementService
    {
        public static List<CommentEditModel> GetCommentsByTaskId(string taskId)
        {

            List<CommentEditModel> commentEditModels;
           
            using (var db = new ErpContext())
            {

                commentEditModels = db.Comment
                    .Where(x => x.TaskId == taskId)
                    .Select(s => new CommentEditModel()
                    {
                        CommentId = s.CommentId,
                        CreatedAt = s.CreatedAt,
                        CreatedBy = s.CreatedBy,
                        TaskId = s.TaskId,
                        Message = s.Message
                    }).ToList();
            }
            return commentEditModels.OrderByDescending(x =>
                x.CreatedAt).ToList();
        }
        
        public static CommentEditModel CreateOrUpdateComment(CommentEditModel commentEditModel)
        {
            Comment comment;
            
            using (var db = new ErpContext())
            {
                comment = db.Comment
                    .FirstOrDefault(x => x.CommentId == commentEditModel.CommentId);


                if (comment != null) // update
                {
                    comment.Message = commentEditModel.Message;
                    comment.TaskId = commentEditModel.TaskId;

                    db.SaveChanges();
                }
                else
                {
                    comment = new Comment()
                    {
                        CommentId = GetNextAvailableCommentId(),
                        TaskId = commentEditModel.TaskId,
                        CreatedAt = DateTime.Now,
                        CreatedBy = commentEditModel.CreatedBy,
                        Message = commentEditModel.Message
                    };
                    db.Comment.Add(comment);
                    db.SaveChanges();
                }
            }

            return GetCommentById(comment.CommentId);
        }
        
        private static CommentEditModel GetCommentById(string commentId)
        {

            CommentEditModel commentEditModel;
            using (var db = new ErpContext())
            {

                commentEditModel = db.Comment
                    .Where(x => x.CommentId == commentId)
                    .Select(s => new CommentEditModel()
                    {
                        CommentId = s.CommentId,
                        CreatedAt = s.CreatedAt,
                        CreatedBy = s.CreatedBy,
                        TaskId = s.TaskId,
                        Message = s.Message
                    }).ToList().FirstOrDefault();
            }

            return commentEditModel;
        }
        private static string GetNextAvailableCommentId()
        {
            using (var db = new ErpContext())
            {
                var a = db.Comment
                    .Select(x => Convert.ToInt32(x.CommentId))
                    .DefaultIfEmpty(0)
                    .Max();
                return Convert.ToString(a + 1);
            }
          
        }
        
        public static void DeleteComment(string commentId)
        {
            using (var db = new ErpContext())
            {
                // Get Selected Dependency
                Comment existingComment = db.Comment
                    .FirstOrDefault(x => x.CommentId == commentId);
                if (existingComment != null)
                {
                    db.Comment.Remove(existingComment); 
                    db.SaveChanges();
                }
            }
        }

    }
}