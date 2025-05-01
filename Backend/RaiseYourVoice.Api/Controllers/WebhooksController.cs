using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RaiseYourVoice.Application.Interfaces;
using RaiseYourVoice.Domain.Enums;
using RaiseYourVoice.Infrastructure.Services;
using Stripe;
using Stripe.Checkout;
using System.IO;
using System.Threading.Tasks;

namespace RaiseYourVoice.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhooksController : ControllerBase
    {
        private readonly ILogger<WebhooksController> _logger;
        private readonly IDonationService _donationService;
        private readonly ICampaignService _campaignService;
        private readonly IPushNotificationService _notificationService;
        private readonly string _endpointSecret;

        public WebhooksController(
            ILogger<WebhooksController> logger,
            IDonationService donationService,
            ICampaignService campaignService,
            IPushNotificationService notificationService,
            IOptions<StripeSettings> stripeSettings)
        {
            _logger = logger;
            _donationService = donationService;
            _campaignService = campaignService;
            _notificationService = notificationService;
            _endpointSecret = stripeSettings.Value.WebhookSecret;
        }

        [HttpPost]
        public async Task<IActionResult> Index()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    _endpointSecret
                );

                // Handle the event
                // switch (stripeEvent.Type)
                // {
                //     case Stripe.Events.PaymentIntentSucceeded:
                //         var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                //         await HandlePaymentIntentSucceeded(paymentIntent);
                //         break;
                //     case Stripe.Events.PaymentIntentPaymentFailed:
                //         var failedPaymentIntent = stripeEvent.Data.Object as PaymentIntent;
                //         await HandlePaymentIntentFailed(failedPaymentIntent);
                //         break;
                //     case Stripe.Events.CustomerSubscriptionCreated:
                //         var subscription = stripeEvent.Data.Object as Subscription;
                //         await HandleSubscriptionCreated(subscription);
                //         break;
                //     case Stripe.Events.CustomerSubscriptionUpdated:
                //         var updatedSubscription = stripeEvent.Data.Object as Subscription;
                //         await HandleSubscriptionUpdated(updatedSubscription);
                //         break;
                //     case Stripe.Events.CustomerSubscriptionDeleted:
                //         var deletedSubscription = stripeEvent.Data.Object as Subscription;
                //         await HandleSubscriptionCancelled(deletedSubscription);
                //         break;
                //     case Stripe.Events.InvoicePaid:
                //         var invoice = stripeEvent.Data.Object as Invoice;
                //         await HandleInvoicePaid(invoice);
                //         break;
                //     case Stripe.Events.InvoicePaymentFailed:
                //         var failedInvoice = stripeEvent.Data.Object as Invoice;
                //         await HandleInvoicePaymentFailed(failedInvoice);
                //         break;
                //     default:
                //         _logger.LogInformation("Unhandled event type: {0}", stripeEvent.Type);
                //         break;
                // }

                return Ok();
            }
            catch (StripeException e)
            {
                _logger.LogError(e, "Error processing Stripe webhook");
                return BadRequest();
            }
        }

        private async Task HandlePaymentIntentSucceeded(PaymentIntent paymentIntent)
        {
            _logger.LogInformation("Payment succeeded: {0}", paymentIntent.Id);

            // Find the donation record by transaction ID
            var donations = await _donationService.GetDonationsByTransactionIdAsync(paymentIntent.Id);
            
            foreach (var donation in donations)
            {
                // Update the donation status if it's not already completed
                if (donation.PaymentStatus != PaymentStatus.Completed)
                {
                    await _donationService.UpdateDonationStatusAsync(donation.Id, PaymentStatus.Completed);
                    
                    // Notify the user
                    if (!string.IsNullOrEmpty(donation.UserId))
                    {
                        await _notificationService.SendNotificationAsync(
                            donation.UserId,
                            "Thank you for your donation!",
                            $"Your donation of {donation.Amount:C} has been processed successfully."
                        );
                    }
                }
            }
        }

        private async Task HandlePaymentIntentFailed(PaymentIntent paymentIntent)
        {
            _logger.LogInformation("Payment failed: {0}", paymentIntent.Id);

            // Find the donation record by transaction ID
            var donations = await _donationService.GetDonationsByTransactionIdAsync(paymentIntent.Id);
            
            foreach (var donation in donations)
            {
                // Update the donation status
                await _donationService.UpdateDonationStatusAsync(donation.Id, PaymentStatus.Failed);
                
                // Notify the user
                if (!string.IsNullOrEmpty(donation.UserId))
                {
                    await _notificationService.SendNotificationAsync(
                        donation.UserId,
                        "Donation payment failed",
                        $"Your donation payment of {donation.Amount:C} could not be processed. Please check your payment details and try again."
                    );
                }
            }
        }

        private async Task HandleSubscriptionCreated(Subscription subscription)
        {
            _logger.LogInformation("Subscription created: {0}", subscription.Id);
            
            // This is generally handled in our direct API call, but we'll check if we need to update anything
            var customerId = subscription.CustomerId;
            
            // Additional logic if needed
        }

        private async Task HandleSubscriptionUpdated(Subscription subscription)
        {
            _logger.LogInformation("Subscription updated: {0}", subscription.Id);
            
            // Handle subscription updates (e.g., upgraded/downgraded plans, payment method changes)
            // This might involve updating our records or notifying the user
        }

        private async Task HandleSubscriptionCancelled(Subscription subscription)
        {
            _logger.LogInformation("Subscription cancelled: {0}", subscription.Id);
            
            // Update our records to reflect cancellation
            await _donationService.CancelSubscriptionDonationAsync(subscription.Id);
            
            // Notify the user
            if (subscription.Metadata.TryGetValue("userId", out var userId) && !string.IsNullOrEmpty(userId))
            {
                await _notificationService.SendNotificationAsync(
                    userId,
                    "Subscription Cancelled",
                    "Your recurring donation subscription has been cancelled."
                );
            }
        }

        private async Task HandleInvoicePaid(Invoice invoice)
        {
            _logger.LogInformation("Invoice paid: {0}", invoice.Id);
            
            // This handles recurring subscription payments
            // if (!string.IsNullOrEmpty(invoice.SubscriptionId))
            // {
                // Get the subscription details
                var subscriptionService = new SubscriptionService();
                // var subscription = await subscriptionService.GetAsync(invoice.SubscriptionId);
                
                // Get the campaign ID from metadata (assuming it was stored there)
                // if (!subscription.Metadata.TryGetValue("campaignId", out var campaignId))
                // {
                //     _logger.LogWarning("Campaign ID not found in subscription metadata");
                //     return;
                // }
                
                // Create a donation record for this invoice payment
                // if (invoice.Metadata.TryGetValue("userId", out var userId))
                // {
                //     // Create a new donation for this invoice payment
                //     var donation = new RaiseYourVoice.Domain.Entities.Donation
                //     {
                //         CampaignId = campaignId,
                //         UserId = userId,
                //         Amount = invoice.AmountPaid / 100m, // Stripe amounts are in cents
                //         IsAnonymous = false,
                //         PaymentStatus = PaymentStatus.Completed,
                //         PaymentMethod = "card", // Assuming card payment through subscription
                //         Currency = invoice.Currency,
                //         // TransactionId = invoice.PaymentIntentId, // Use PaymentIntentId instead of ChargeId
                //         IsSubscriptionDonation = true,
                //         // SubscriptionId = invoice.SubscriptionId,
                //         CreatedAt = DateTime.UtcNow
                //     };
                    
                //     await _donationService.CreateDonationAsync(donation);
                // }
            // }
        }

        private async Task HandleInvoicePaymentFailed(Invoice invoice)
        {
            _logger.LogInformation("Invoice payment failed: {0}", invoice.Id);
            
            // Notify the user of failed subscription payment
            if (invoice.Metadata.TryGetValue("userId", out var userId))
            {
                await _notificationService.SendNotificationAsync(
                    userId,
                    "Subscription Payment Failed",
                    "Your recurring donation payment could not be processed. Please update your payment method to continue supporting this cause."
                );
            }
        }
    }
}