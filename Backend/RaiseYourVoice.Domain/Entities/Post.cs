using System;
using System.Collections.Generic;
using RaiseYourVoice.Domain.Common;
using RaiseYourVoice.Domain.Enums;

namespace RaiseYourVoice.Domain.Entities
{
    public class Post : BaseEntity
    {
        public required string Title { get; set; }
        public required string Content { get; set; }
        public List<string> MediaUrls { get; set; } = new List<string>();
        public PostType PostType { get; set; }
        public required string AuthorId { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public Location? Location { get; set; }
        public DateTime? EventDate { get; set; }
        public PostStatus Status { get; set; }
    }

    public class Location
    {
        public string? Address { get; set; }
        public required string City { get; set; }
        public required string Country { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}