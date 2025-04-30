using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using RaiseYourVoice.Application.Interfaces;
using RaiseYourVoice.Domain.Entities;
using Stripe;
using PaymentStatus = RaiseYourVoice.Domain.Enums.PaymentStatus;

namespace RaiseYourVoice.Infrastructure.Services
{
    public class StripePaymentGateway : IPaymentGateway
    {
        private readonly StripeSettings _stripeSettings;

        public StripePaymentGateway(IOptions<StripeSettings> stripeSettings)
        {
            _stripeSettings = stripeSettings.Value;
            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
        }

        public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
        {
            try
            {
                // Create customer if needed
                string customerId = await CreateCustomerAsync(request.CustomerInfo);
                
                // Create payment method if tokenId is not provided
                var paymentMethodId = request.PaymentMethod.TokenId;
                if (string.IsNullOrEmpty(paymentMethodId))
                {
                    paymentMethodId = await CreatePaymentMethodAsync(request.PaymentMethod);
                    
                    if (request.SavePaymentMethod)
                    {
                        // Attach payment method to customer
                        await SavePaymentMethodAsync(customerId, request.PaymentMethod);
                    }
                }

                // Create payment intent
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(request.Amount * 100), // Stripe amounts are in cents
                    Currency = request.Currency.ToLower(),
                    Description = request.Description,
                    Customer = customerId,
                    PaymentMethod = paymentMethodId,
                    Confirm = true,
                    ReceiptEmail = request.CustomerInfo.Email,
                    Metadata = new System.Collections.Generic.Dictionary<string, string>
                    {
                        { "campaign_id", request.CampaignId },
                        { "customer_name", request.CustomerInfo.FullName }
                    }
                };

                var service = new PaymentIntentService();
                var paymentIntent = await service.CreateAsync(options);

                return new PaymentResult
                {
                    Success = paymentIntent.Status == "succeeded",
                    TransactionId = paymentIntent.Id,
                    Status = MapStripeStatusToPaymentStatus(paymentIntent.Status),
                    ReceiptUrl = paymentIntent.Charges?.Data?[0]?.ReceiptUrl,
                    CustomerId = customerId,
                    PaymentMethodId = paymentMethodId
                };
            }
            catch (StripeException e)
            {
                return new PaymentResult
                {
                    Success = false,
                    ErrorMessage = e.Message,
                    Status = PaymentStatus.Failed
                };
            }
        }

        public async Task<PaymentResult> RefundPaymentAsync(string transactionId, decimal amount, string reason)
        {
            try
            {
                var service = new RefundService();
                var refundOptions = new RefundCreateOptions
                {
                    PaymentIntent = transactionId,
                    Amount = (long)(amount * 100), // Convert to cents
                    Reason = MapRefundReason(reason)
                };

                var refund = await service.CreateAsync(refundOptions);
                
                return new PaymentResult
                {
                    Success = refund.Status == "succeeded",
                    TransactionId = refund.Id,
                    Status = refund.Status == "succeeded" ? PaymentStatus.Refunded : PaymentStatus.Failed,
                    ErrorMessage = refund.FailureReason
                };
            }
            catch (StripeException e)
            {
                return new PaymentResult
                {
                    Success = false,
                    ErrorMessage = e.Message,
                    Status = PaymentStatus.Failed
                };
            }
        }

        public async Task<string> CreateCustomerAsync(DonorInformation customerInfo)
        {
            var customerService = new CustomerService();
            var customerCreateOptions = new CustomerCreateOptions
            {
                Email = customerInfo.Email,
                Name = customerInfo.FullName,
                Phone = customerInfo.Phone,
                Address = new AddressOptions
                {
                    Line1 = customerInfo.Address,
                    City = customerInfo.City,
                    State = customerInfo.State,
                    Country = customerInfo.Country,
                    PostalCode = customerInfo.PostalCode
                }
            };

            var customer = await customerService.CreateAsync(customerCreateOptions);
            return customer.Id;
        }

        public async Task<string> SavePaymentMethodAsync(string customerId, PaymentMethodInfo paymentMethod)
        {
            if (string.IsNullOrEmpty(paymentMethod.TokenId))
            {
                // Create new payment method
                var paymentMethodId = await CreatePaymentMethodAsync(paymentMethod);
                
                // Attach to customer
                var paymentMethodService = new PaymentMethodService();
                await paymentMethodService.AttachAsync(
                    paymentMethodId,
                    new PaymentMethodAttachOptions { Customer = customerId }
                );
                
                return paymentMethodId;
            }
            else
            {
                // Token already exists, just attach it
                var paymentMethodService = new PaymentMethodService();
                await paymentMethodService.AttachAsync(
                    paymentMethod.TokenId,
                    new PaymentMethodAttachOptions { Customer = customerId }
                );
                
                return paymentMethod.TokenId;
            }
        }

        public async Task<string> CreateSubscriptionAsync(string customerId, string paymentMethodId, decimal amount, string description)
        {
            // Create a product for this subscription if it doesn't exist
            var productService = new ProductService();
            var product = await productService.CreateAsync(new ProductCreateOptions
            {
                Name = "Monthly Donation",
                Description = description
            });

            // Create a price for the product
            var priceService = new PriceService();
            var price = await priceService.CreateAsync(new PriceCreateOptions
            {
                Product = product.Id,
                UnitAmount = (long)(amount * 100), // Convert to cents
                Currency = "usd",
                Recurring = new PriceRecurringOptions
                {
                    Interval = "month",
                }
            });

            // Set the default payment method for the customer
            var customerService = new CustomerService();
            await customerService.UpdateAsync(customerId, new CustomerUpdateOptions
            {
                InvoiceSettings = new CustomerInvoiceSettingsOptions
                {
                    DefaultPaymentMethod = paymentMethodId
                }
            });

            // Create the subscription
            var subscriptionService = new SubscriptionService();
            var subscription = await subscriptionService.CreateAsync(new SubscriptionCreateOptions
            {
                Customer = customerId,
                Items = new System.Collections.Generic.List<SubscriptionItemOptions>
                {
                    new SubscriptionItemOptions
                    {
                        Price = price.Id
                    }
                },
                DefaultPaymentMethod = paymentMethodId,
                Description = description
            });

            return subscription.Id;
        }

        public async Task<bool> CancelSubscriptionAsync(string subscriptionId)
        {
            try
            {
                var service = new SubscriptionService();
                await service.CancelAsync(subscriptionId, new SubscriptionCancelOptions());
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<PaymentStatus> GetPaymentStatusAsync(string transactionId)
        {
            var service = new PaymentIntentService();
            var paymentIntent = await service.GetAsync(transactionId);
            return MapStripeStatusToPaymentStatus(paymentIntent.Status);
        }

        private PaymentStatus MapStripeStatusToPaymentStatus(string stripeStatus)
        {
            return stripeStatus switch
            {
                "succeeded" => PaymentStatus.Completed,
                "processing" => PaymentStatus.Pending,
                "requires_payment_method" => PaymentStatus.Failed,
                "requires_action" => PaymentStatus.Pending,
                "requires_capture" => PaymentStatus.Pending,
                "canceled" => PaymentStatus.Cancelled,
                _ => PaymentStatus.Failed
            };
        }

        private string MapRefundReason(string reason)
        {
            // Maps our refund reason to Stripe-specific reasons
            return reason.ToLower() switch
            {
                "requested_by_customer" => "requested_by_customer",
                "duplicate" => "duplicate",
                "fraudulent" => "fraudulent",
                _ => "requested_by_customer" // Default
            };
        }

        private async Task<string> CreatePaymentMethodAsync(PaymentMethodInfo paymentMethod)
        {
            var service = new PaymentMethodService();
            var options = new PaymentMethodCreateOptions
            {
                Type = "card", // Currently only supporting card
                Card = new PaymentMethodCardOptions
                {
                    Number = paymentMethod.CardNumber,
                    ExpMonth = long.Parse(paymentMethod.ExpiryMonth),
                    ExpYear = long.Parse(paymentMethod.ExpiryYear),
                    Cvc = paymentMethod.Cvc
                },
                BillingDetails = new PaymentMethodBillingDetailsOptions
                {
                    Name = paymentMethod.CardholderName
                }
            };

            var result = await service.CreateAsync(options);
            return result.Id;
        }
    }

    public class StripeSettings
    {
        public string SecretKey { get; set; }
        public string PublishableKey { get; set; }
        public string WebhookSecret { get; set; }
    }
}