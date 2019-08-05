using System.Configuration;

namespace Verndale.CognitiveImageTagging.Services
{
	/// <summary>
	/// The application settings as set in the app.config or web.config
	/// </summary>
	public class AzureServiceConfiguration : ConfigurationSection
	{
		/// <summary>
		/// The currently loaded settings
		/// </summary>
		public static AzureServiceConfiguration Current
		{
			get
			{
				return ConfigurationManager.GetSection("cognitiveImageTagging/azureService") as AzureServiceConfiguration;
			}
		}

		/// <summary>
		/// The Azure Cognitive Services Subscription Key
		/// </summary>
		[ConfigurationProperty("subscriptionKey", IsRequired = true)]
		public string SubscriptionKey
		{
			get => this["subscriptionKey"].ToString();
			set => this["subscriptionKey"] = value;
		}


		/// <summary>
		/// The URL for Azure Cognitive Services associated with the Subscription Key (note that the key and the URL are a pair)
		/// </summary>
		[ConfigurationProperty("cognitiveServicesUri", IsRequired = true)]
		public string CognitiveServicesUri
		{
			get => this["cognitiveServicesUri"].ToString();
			set => this["cognitiveServicesUri"] = value;
		}

		/// <summary>
		/// The Visual Features parameters to pass to Azure Cognitive Services for evaluation. Available options include:  Description,Color,Tags,Brands,Adult,Faces,ImageType,Objects
		/// </summary>
		[ConfigurationProperty("visualFeatures", IsRequired = false, DefaultValue = "Description")]
		public string VisualFeatures
		{
			get => this["visualFeatures"].ToString();
			set => this["visualFeatures"] = value;
		}

		/// <summary>
		/// The Image Details parameters to pass to Azure Cognitive Services for evaluation. Available options include: Celebrity, Landmarks
		/// </summary>
		[ConfigurationProperty("details", IsRequired = false
			, DefaultValue = "")]
		public string ImageDetails
		{
			get => this["details"].ToString();
			set => this["details"] = value;
		}

		/// <summary>
		/// Specifies whether Azure Cognitive Services should detect the orientation of the image text and correct
		/// </summary>
		[ConfigurationProperty("detectOrientation", IsRequired = false
			, DefaultValue = "true")]
		public bool DetectOrientation
		{
			get => (bool)this["detectOrientation"];
			set => this["detectOrientation"] = value;
		}

		/// <summary>
		/// The Text Check Mode parameter to pass to Azure Cognitive Services to help determine if the image contains text. Default is "Printed"
		/// </summary>
		[ConfigurationProperty("textCheckMode", IsRequired = false
			, DefaultValue = "Printed")]
		public string TextCheckMode
		{
			get => this["textCheckMode"].ToString();
			set => this["textCheckMode"] = value;
		}

		/// <summary>
		/// The "Confidence Level" returned from Azure that should indicate that a given image contains text. Scores equal or greater than this will assume the image does contain text.
		/// </summary>
		[ConfigurationProperty("textInImageConfidenceLevel", DefaultValue = .9, IsRequired = false)]
		public double TextInImageConfidenceLevel
		{
			get => (double)this["textInImageConfidenceLevel"];
			set => this["textInImageConfidenceLevel"] = value;
		}

		/// <summary>
		/// The "Confidence Level" returned form Azure that should indicate whether a descriptive word returned from Azure has meaning in the current image. Scores equal or greater to this value will indicate the text should be included in output.
		/// </summary>
		[ConfigurationProperty("captionConfidenceLevel", DefaultValue = .75, IsRequired = false)]
		public double CaptionConfidenceLevel
		{
			get => (double)this["captionConfidenceLevel"];
			set => this["captionConfidenceLevel"] = value;
		}
	}
}