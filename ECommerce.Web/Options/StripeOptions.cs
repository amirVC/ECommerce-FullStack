namespace ECommerce.Web.Options
{
    public class StripeOptions
    {
        public const string SectionName = "Stripe";

        // Publishable key only -- this one is safe to ship to the browser.
        // The secret key lives only in the API project's user-secrets.
        public string PublishableKey { get; set; } = string.Empty;
    }
}
