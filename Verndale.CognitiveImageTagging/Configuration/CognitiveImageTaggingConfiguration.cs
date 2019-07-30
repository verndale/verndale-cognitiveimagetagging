using System.Configuration;

namespace Verndale.CognitiveImageTagging.Configuration
{
    public class CognitiveImageTaggingConfiguration : ConfigurationSection
    {
        private static CognitiveImageTaggingConfiguration _settings
            = ConfigurationManager.GetSection("cognitiveImageTagging") as CognitiveImageTaggingConfiguration;

        public static CognitiveImageTaggingConfiguration Settings => _settings;

        [ConfigurationProperty("subscriptionKey", IsRequired = true)]
        public string SubscriptionKey
        {
            get => this["subscriptionKey"].ToString();
            set => this["subscriptionKey"] = value;
        }

        [ConfigurationProperty("cognitiveServicesUri", IsRequired = true)]
        public string CognitiveServicesUri
        {
            get => this["cognitiveServicesUri"].ToString();
            set => this["cognitiveServicesUri"] = value;
        }

        [ConfigurationProperty("visualFeatures", IsRequired = false
        ,DefaultValue = "Description,Tags")]
        public string VisualFeatures
        {
            get => this["visualFeatures"].ToString();
            set => this["visualFeatures"] = value;
        }

        [ConfigurationProperty("details", IsRequired = false
            , DefaultValue = "")]
        public string ImageDetails
        {
            get => this["details"].ToString();
            set => this["details"] = value;
        }

        [ConfigurationProperty("detectOrientation", IsRequired = false
            , DefaultValue = "true")]
        public bool DetectOrientation
        {
            get => (bool)this["detectOrientation"];
            set => this["detectOrientation"] = value;
        }

        [ConfigurationProperty("textCheckMode", IsRequired = false
            , DefaultValue = "Printed")]
        public string TextCheckMode
        {
            get => this["textCheckMode"].ToString();
            set => this["textCheckMode"] = value;
        }

        [ConfigurationProperty("textInImageConfidenceLevel", DefaultValue = .9, IsRequired = false)]
        public double TextInImageConfidenceLevel
        {
            get => (double)this["textInImageConfidenceLevel"];
            set => this["textInImageConfidenceLevel"] = value;
        }

        [ConfigurationProperty("captionConfidenceLevel", DefaultValue = .75, IsRequired = false)]
        public double CaptionConfidenceLevel
        {
            get => (double)this["captionConfidenceLevel"];
            set => this["captionConfidenceLevel"] = value;
        }
    }
}