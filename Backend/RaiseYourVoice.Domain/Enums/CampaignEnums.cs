namespace RaiseYourVoice.Domain.Enums
{
    public enum CampaignStatus
    {
        Draft,
        PendingApproval,
        Active,
        Paused,
        Completed,
        Cancelled,
        Rejected
    }

    public enum CampaignCategory
    {
        Education,
        Health,
        Environment,
        HumanRights,
        AnimalRights,
        Disaster,
        Arts,
        Community,
        Technology,
        Sports,
        Other
    }

    public enum PaymentStatus
    {
        Pending,
        Completed,
        Failed,
        Refunded,
        Cancelled,
        Disputed
    }
}