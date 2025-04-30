namespace RaiseYourVoice.Domain.Enums
{
    public enum NotificationType
    {
        PostCreated,
        CommentAdded,
        EventReminder,
        FundingOpportunity,
        SystemAnnouncement,
        VerificationUpdate,
        UserMention
    }

    public enum DeliveryStatus
    {
        Queued,
        Sent,
        Failed
    }

    public enum ReadStatus
    {
        Unread,
        Read,
        Dismissed
    }

    public enum TargetType
    {
        AllUsers,
        SpecificUsers,
        ByRole,
        ByTopic,
        ByRegion
    }
}