using System;
using RaiseYourVoice.Domain.Common;

namespace RaiseYourVoice.Domain.Entities
{
    public class Comment : BaseEntity
    {
        public required string PostId { get; set; }
        public required string AuthorId { get; set; }
        public required string Content { get; set; }
        public int LikeCount { get; set; }
        public string? ParentCommentId { get; set; }
    }
}