namespace RaiseYourVoice.Application.Models.Requests
{
    /// <summary>
    /// Request model for featuring or unfeaturing a campaign
    /// </summary>
    public class FeatureRequest
    {
        /// <summary>
        /// Whether the campaign should be featured
        /// </summary>
        public bool Featured { get; set; }
    }
}