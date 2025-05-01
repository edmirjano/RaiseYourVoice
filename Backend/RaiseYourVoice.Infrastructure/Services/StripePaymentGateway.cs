using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaiseYourVoice.Application.Interfaces;
using RaiseYourVoice.Application.Models.Requests;
using RaiseYourVoice.Domain.Entities;
using RaiseYourVoice.Domain.Enums;
using Stripe;
using System.Text.Json;
using MongoDB.Bson;

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

        public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
        {
            try
            {
                // Create payment using Stripe's Payment Intents API
                var options = new PaymentIntentCreateOptions
                {
                    Amount = ConvertToStripeAmount(request.Amount, request.Currency),
                    Currency = request.Currency.ToLower(),
                    Description = request.Description,
                    PaymentMethod = request.PaymentMethod.TokenId,
                    ConfirmationMethod = "automatic",
                    Confirm = true,
                    ReceiptEmail = request.CustomerInfo?.Email,
                    Metadata = new Dictionary<string, string>
                    {
                        { "campaignId", request.CampaignId }
                    }
                };

                var service = new PaymentIntentService();
                var paymentIntent = await service.CreateAsync(options);
                
                return new PaymentResult
                {
                    Success = paymentIntent.Status == "succeeded",
                    TransactionId = paymentIntent.Id,
                    Status = TranslateToPaymentStatus(paymentIntent.Status),
                    ReceiptUrl = null, // Stripe's PaymentIntent doesn't directly provide receipt URL
                    CustomerId = paymentIntent.CustomerId,
                    PaymentMethodId = paymentIntent.PaymentMethodId,
                    ErrorMessage = null
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error processing payment: {Message}", ex.Message);
                return new PaymentResult
                {
                    Success = false,
                    TransactionId = null,
                    Status = PaymentStatus.Failed,
                    ErrorMessage = ex.Message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment: {Message}", ex.Message);
                return new PaymentResult
                {
                    Success = false,
                    TransactionId = null,
                    Status = PaymentStatus.Failed,
                    ErrorMessage = "An unexpected error occurred"
                };
            }
        }

        public async Task<PaymentResult> RefundPaymentAsync(string transactionId, decimal amount, string reason)
        {
            try
            {
                var options = new RefundCreateOptions
                {
                    PaymentIntent = transactionId,
                    Amount = ConvertToStripeAmount(amount, "usd"), // Default to USD
                    Reason = TranslateRefundReason(reason)
                };
                
                var service = new RefundService();
                var refund = await service.CreateAsync(options);
                
                return new PaymentResult
                {
                    Success = refund.Status == "succeeded",
                    TransactionId = refund.Id,
                    Status = TranslateToPaymentStatus(refund.Status),
                    ErrorMessage = null
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error processing refund: {Message}", ex.Message);
                return new PaymentResult
                {
                    Success = false,
                    TransactionId = null,
                    Status = PaymentStatus.Failed,
                    ErrorMessage = ex.Message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing refund: {Message}", ex.Message);
                return new PaymentResult
                {
                    Success = false,
                    TransactionId = null,
                    Status = PaymentStatus.Failed,
                    ErrorMessage = "An unexpected error occurred"
                };
            }
        }

        public async Task<string> CreateCustomerAsync(DonorInformation customerInfo)
        {
            try
            {
                var customerOptions = new CustomerCreateOptions
                {
                    Email = customerInfo.Email,
                    Name = customerInfo.FullName,
                    Metadata = new Dictionary<string, string>
                    {
                        { "userId", ObjectId.GenerateNewId().ToString() }
                    }
                };
                
                if (customerInfo.Address != null)
                {
                    customerOptions.Address = new AddressOptions
                    {
                        Line1 = customerInfo.Address,
                        City = customerInfo.City,
                        State = customerInfo.State,
                        PostalCode = customerInfo.PostalCode,
                        Country = customerInfo.Country
                    };
                }
                
                var customerService = new CustomerService();
                var customer = await customerService.CreateAsync(customerOptions);
                return customer.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Stripe customer: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<string> SavePaymentMethodAsync(string customerId, PaymentMethodInfo paymentMethod)
        {
            try
            {
                // Create a payment method
                var paymentMethodOptions = new PaymentMethodCreateOptions
                {
                    Type = paymentMethod.Type,
                    Card = new PaymentMethodCardOptions
                    {
                        Number = paymentMethod.CardNumber,
                        ExpMonth = int.Parse(paymentMethod.ExpiryMonth),
                        ExpYear = int.Parse(paymentMethod.ExpiryYear),
                        Cvc = paymentMethod.Cvc
                    },
                    BillingDetails = new PaymentMethodBillingDetailsOptions
                    {
                        Name = paymentMethod.CardholderName
                    }
                };
                
                var paymentMethodService = new PaymentMethodService();
                var createdPaymentMethod = await paymentMethodService.CreateAsync(paymentMethodOptions);
                
                // Attach the payment method to the customer
                var attachOptions = new PaymentMethodAttachOptions
                {
                    Customer = customerId
                };
                
                await paymentMethodService.AttachAsync(createdPaymentMethod.Id, attachOptions);
                
                return createdPaymentMethod.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving payment method: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<string> CreateSubscriptionAsync(string customerId, string paymentMethodId, decimal amount, string description)
        {
            try
            {
                // Create or retrieve a price for the subscription
                var priceService = new PriceService();
                var priceOptions = new PriceCreateOptions
                {
                    UnitAmount = ConvertToStripeAmount(amount, "usd"),
                    Currency = "usd",
                    Recurring = new PriceRecurringOptions
                    {
                        Interval = "month"
                    },
                    Product = await GetOrCreateProductId(description)
                };
                
                var price = await priceService.CreateAsync(priceOptions);
                
                // Set the default payment method for the customer
                var customerService = new CustomerService();
                var customerUpdateOptions = new CustomerUpdateOptions
                {
                    InvoiceSettings = new CustomerInvoiceSettingsOptions
                    {
                        DefaultPaymentMethod = paymentMethodId
                    }
                };
                
                await customerService.UpdateAsync(customerId, customerUpdateOptions);
                
                // Create the subscription
                var subscriptionOptions = new SubscriptionCreateOptions
                {
                    Customer = customerId,
                    Items = new List<SubscriptionItemOptions>
                    {
                        new SubscriptionItemOptions
                        {
                            Price = price.Id,
                        },
                    },
                    PaymentSettings = new SubscriptionPaymentSettingsOptions
                    {
                        SaveDefaultPaymentMethod = "on_subscription"
                    },
                    Metadata = new Dictionary<string, string>
                    {
                        { "description", description }
                    }
                };

                var subscriptionService = new SubscriptionService();
                var subscription = await subscriptionService.CreateAsync(subscriptionOptions);
                
                return subscription.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating subscription: {Message}", ex.Message);
                throw;
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

        public async Task<PaymentStatus> GetPaymentStatusAsync(string transactionId)
        {
            try
            {
                var service = new PaymentIntentService();
                var paymentIntent = await service.GetAsync(transactionId);
                
                return TranslateToPaymentStatus(paymentIntent.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment status: {Message}", ex.Message);
                throw;
            }
        }

        private async Task<string> GetOrCreateProductId(string description)
        {
            // Create a simple product for the subscription
            var productName = "Monthly Donation";
            var productService = new ProductService();
            var productOptions = new ProductCreateOptions
            {
                Name = productName,
                Description = description
            };
            
            var product = await productService.CreateAsync(productOptions);
            return product.Id;
        }

        private long ConvertToStripeAmount(decimal amount, string currency)
        {
            // Stripe requires amounts to be in the smallest currency unit (e.g. cents for USD)
            return Convert.ToInt64(amount * 100);
        }

        private PaymentStatus TranslateToPaymentStatus(string stripeStatus)
        {
            return stripeStatus switch
            {
                "succeeded" => PaymentStatus.Completed,
                "processing" => PaymentStatus.Pending, // Assuming Pending exists in your enum
                "requires_payment_method" => PaymentStatus.Failed,
                "requires_action" => PaymentStatus.Pending,
                "requires_confirmation" => PaymentStatus.Pending,
                "requires_capture" => PaymentStatus.Pending,
                "canceled" => PaymentStatus.Cancelled,
                _ => PaymentStatus.Pending
            };
        }

        private string TranslateRefundReason(string reason)
        {
            return reason.ToLower() switch
            {
                "requested_by_customer" => "requested_by_customer",
                "duplicate" => "duplicate",
                "fraudulent" => "fraudulent",
                _ => "other"
            };
        }
    }

    public class StripeSettings
    {
        public string SecretKey { get; set; } = string.Empty;
        public string PublishableKey { get; set; } = string.Empty;
        public string WebhookSecret { get; set; } = string.Empty;
    }
}