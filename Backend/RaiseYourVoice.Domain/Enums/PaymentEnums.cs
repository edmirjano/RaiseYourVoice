namespace RaiseYourVoice.Domain.Enums
{
    public enum PaymentStatus
    {
        Pending,
        Completed,
        Failed,
        Refunded,
        Disputed,
        Cancelled
    }
    
    public enum PaymentMethod
    {
        CreditCard,
        PayPal,
        BankTransfer,
        ApplePay,
        GooglePay,
        Cryptocurrency,
        Other
    }
    
    public enum PaymentFrequency
    {
        OneTime,
        Monthly,
        Quarterly,
        Annually
    }
}