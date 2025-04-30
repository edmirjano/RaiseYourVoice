using System;
using RaiseYourVoice.Domain.Common;

namespace RaiseYourVoice.Domain.Entities
{
    public class Comment : BaseEntity
    {
        public string PostId { get; set; }
        public string AuthorId { get; set; }
        public string Content { get; set; }
        public int LikeCount { get; set; }
        public string ParentCommentId { get; set; }
    }
}