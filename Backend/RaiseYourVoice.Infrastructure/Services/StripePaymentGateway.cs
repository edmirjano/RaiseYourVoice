using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaiseYourVoice.Application.Interfaces;
using RaiseYourVoice.Application.Models.Requests;
using RaiseYourVoice.Application.Models.Responses;
using Stripe;

namespace RaiseYourVoice.Infrastructure.Services
{
    public class StripePaymentGateway : IPaymentGateway
    {
        private readonly ILogger<StripePaymentGateway> _logger;
        private readonly StripeSettings _stripeSettings;

        public StripePaymentGateway(
            IOptions<StripeSettings> stripeSettings,
            ILogger<StripePaymentGateway> logger)
        {
            _stripeSettings = stripeSettings.Value;
            _logger = logger;
            
            // Configure Stripe API key
            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
        }

        public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request)
        {
            try
            {
                // Create payment using Stripe's Payment Intents API
                var options = new PaymentIntentCreateOptions
                {
                    Amount = ConvertToStripeAmount(request.Amount, request.Currency),
                    Currency = request.Currency.ToLower(),
                    Description = request.Description,
                    PaymentMethod = request.PaymentMethodId,
                    ConfirmationMethod = "automatic",
                    Confirm = true,
                    ReturnUrl = request.ReturnUrl,
                    ReceiptEmail = request.Email,
                    Metadata = new Dictionary<string, string>
                    {
                        { "campaignId", request.CampaignId },
                        { "userId", request.UserId ?? "anonymous" },
                        { "isAnonymous", request.IsAnonymous.ToString() }
                    }
                };

                var service = new PaymentIntentService();
                var paymentIntent = await service.CreateAsync(options);
                
                return new PaymentResponse
                {
                    Success = paymentIntent.Status == "succeeded",
                    TransactionId = paymentIntent.Id,
                    Status = TranslateStripeStatus(paymentIntent.Status),
                    RedirectUrl = paymentIntent.NextAction?.RedirectToUrl?.Url,
                    ErrorMessage = null
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error processing payment: {Message}", ex.Message);
                return new PaymentResponse
                {
                    Success = false,
                    TransactionId = null,
                    Status = "failed",
                    RedirectUrl = null,
                    ErrorMessage = ex.Message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment: {Message}", ex.Message);
                return new PaymentResponse
                {
                    Success = false,
                    TransactionId = null,
                    Status = "failed",
                    RedirectUrl = null,
                    ErrorMessage = "An unexpected error occurred"
                };
            }
        }

        public async Task<PaymentResponse> ProcessRefundAsync(string transactionId, decimal amount, string currency)
        {
            try
            {
                var options = new RefundCreateOptions
                {
                    PaymentIntent = transactionId,
                    Amount = ConvertToStripeAmount(amount, currency)
                };
                
                var service = new RefundService();
                var refund = await service.CreateAsync(options);
                
                return new PaymentResponse
                {
                    Success = refund.Status == "succeeded",
                    TransactionId = refund.Id,
                    Status = TranslateStripeRefundStatus(refund.Status),
                    RedirectUrl = null,
                    ErrorMessage = null
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error processing refund: {Message}", ex.Message);
                return new PaymentResponse
                {
                    Success = false,
                    TransactionId = null,
                    Status = "failed",
                    RedirectUrl = null,
                    ErrorMessage = ex.Message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing refund: {Message}", ex.Message);
                return new PaymentResponse
                {
                    Success = false,
                    TransactionId = null,
                    Status = "failed",
                    RedirectUrl = null,
                    ErrorMessage = "An unexpected error occurred"
                };
            }
        }

        public async Task<SubscriptionResponse> CreateSubscriptionAsync(SubscriptionRequest request)
        {
            try
            {
                // First, check if we need to create a customer
                string customerId = request.CustomerId;
                
                if (string.IsNullOrEmpty(customerId))
                {
                    var customerOptions = new CustomerCreateOptions
                    {
                        Email = request.Email,
                        Name = request.Name,
                        PaymentMethod = request.PaymentMethodId,
                        InvoiceSettings = new CustomerInvoiceSettingsOptions
                        {
                            DefaultPaymentMethod = request.PaymentMethodId,
                        },
                        Metadata = new Dictionary<string, string>
                        {
                            { "userId", request.UserId ?? "anonymous" }
                        }
                    };
                    
                    var customerService = new CustomerService();
                    var customer = await customerService.CreateAsync(customerOptions);
                    customerId = customer.Id;
                }

                // Then create the subscription
                var subscriptionOptions = new SubscriptionCreateOptions
                {
                    Customer = customerId,
                    Items = new List<SubscriptionItemOptions>
                    {
                        new SubscriptionItemOptions
                        {
                            Price = request.PriceId,
                        },
                    },
                    PaymentBehavior = "default_incomplete",
                    PaymentSettings = new SubscriptionPaymentSettingsOptions
                    {
                        SaveDefaultPaymentMethod = "on_subscription",
                    },
                    TrialPeriodDays = request.TrialPeriodDays,
                    Metadata = new Dictionary<string, string>
                    {
                        { "campaignId", request.CampaignId },
                        { "userId", request.UserId ?? "anonymous" }
                    }
                };

                var service = new SubscriptionService();
                var subscription = await service.CreateAsync(subscriptionOptions);
                
                return new SubscriptionResponse
                {
                    Success = true,
                    SubscriptionId = subscription.Id,
                    CustomerId = customerId,
                    Status = TranslateStripeSubscriptionStatus(subscription.Status),
                    CurrentPeriodEnd = subscription.CurrentPeriodEnd,
                    ClientSecret = subscription.LatestInvoice?.PaymentIntent?.ClientSecret,
                    InvoiceUrl = subscription.LatestInvoice?.HostedInvoiceUrl,
                    ErrorMessage = null
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error creating subscription: {Message}", ex.Message);
                return new SubscriptionResponse
                {
                    Success = false,
                    SubscriptionId = null,
                    CustomerId = null,
                    Status = "failed",
                    ErrorMessage = ex.Message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating subscription: {Message}", ex.Message);
                return new SubscriptionResponse
                {
                    Success = false,
                    SubscriptionId = null,
                    CustomerId = null,
                    Status = "failed",
                    ErrorMessage = "An unexpected error occurred"
                };
            }
        }

        public async Task<bool> CancelSubscriptionAsync(string subscriptionId)
        {
            try
            {
                var service = new SubscriptionService();
                await service.CancelAsync(subscriptionId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling subscription: {Message}", ex.Message);
                return false;
            }
        }

        private long ConvertToStripeAmount(decimal amount, string currency)
        {
            // Stripe requires amounts to be in the smallest currency unit (e.g. cents for USD)
            return Convert.ToInt64(amount * 100);
        }

        private string TranslateStripeStatus(string stripeStatus)
        {
            return stripeStatus switch
            {
                "succeeded" => "completed",
                "processing" => "processing",
                "requires_payment_method" => "failed",
                "requires_action" => "pending",
                "requires_confirmation" => "pending",
                "requires_capture" => "pending",
                "canceled" => "cancelled",
                _ => "pending"
            };
        }

        private string TranslateStripeRefundStatus(string stripeStatus)
        {
            return stripeStatus switch
            {
                "succeeded" => "refunded",
                "pending" => "processing",
                "failed" => "failed",
                "canceled" => "cancelled",
                _ => "pending"
            };
        }

        private string TranslateStripeSubscriptionStatus(string stripeStatus)
        {
            return stripeStatus switch
            {
                "active" => "active",
                "past_due" => "past_due",
                "unpaid" => "unpaid",
                "canceled" => "cancelled",
                "incomplete" => "incomplete",
                "incomplete_expired" => "failed",
                "trialing" => "trial",
                "paused" => "paused",
                _ => "pending"
            };
        }
    }

    public class StripeSettings
    {
        public string SecretKey { get; set; }
        public string PublishableKey { get; set; }
        public string WebhookSecret { get; set; }
    }
}