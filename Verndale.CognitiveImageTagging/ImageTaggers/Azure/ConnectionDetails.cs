using System.Configuration;
using System.Text;

namespace Verndale.CognitiveImageTagging.ImageTaggers.Azure
{
	/// <summary>
	/// The application settings as set in the app.config or web.config
	/// </summary>
	public class ConnectionDetails : ConfigurationElement
	{
		#region Locals

		private string _fullEndpointUrl = string.Empty;
		#endregion


		/// <inheritdoc />
		public ConnectionDetails()
		{

		}

		/// <inheritdoc />
		public ConnectionDetails(string name, string subscriptionKey, string cognitiveServicesUri, string visualFeatures,
			string imageDetails, bool detectOrientation, string textCheckMode, double textInImageConfidenceLevel, double captionConfidenceLevel)
		{
			Name = name;
			SubscriptionKey = subscriptionKey;
			CognitiveServicesUri = cognitiveServicesUri;
			VisualFeatures = visualFeatures;
			ImageDetails = imageDetails;
			DetectOrientation = detectOrientation;
			TextCheckMode = textCheckMode;
			TextInImageConfidenceLevel = textInImageConfidenceLevel;
			CaptionConfidenceLevel = captionConfidenceLevel;
		}


		/// <summary>
		/// The name of the connection details that need to be loaded.
		/// </summary>
		[ConfigurationProperty("name", IsRequired = true)]
		public string Name
		{
			get => this["name"].ToString();
			set => this["name"] = value;
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

		[ConfigurationProperty("endpointName", IsRequired = true)]
		public string EndpointName
		{
			get => this["endpointName"].ToString();
			set => this["endpointName"] = value;
		}

		[ConfigurationProperty("serviceVersionNumber", IsRequired = true)]
		public string ServiceVersionNumber
		{
			get => this["serviceVersionNumber"].ToString();
			set => this["serviceVersionNumber"] = value;
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

		public string GetAnalyzeUrl(string languageCode)
		{
			var requestParameters =
				$"visualFeatures={VisualFeatures}&details={ImageDetails}&language={languageCode}&detectOrientation={DetectOrientation}";

			return GetFullEndpointUrl() + "/analyze?" + requestParameters;
		}

		public string GetOcrUrl()
		{
			return $"{GetFullEndpointUrl()}/recognizeText?mode={TextCheckMode}";
		}


		/// <summary>
		/// Concatenates CognitiveServicesUri, EndpointName and ServiceVersionNumber into a valid request URL.
		/// </summary>
		/// <returns>A URL that can be used to query the Azure service.</returns>
		public string GetFullEndpointUrl()
		{
			if (!string.IsNullOrEmpty(_fullEndpointUrl))
			{
				return _fullEndpointUrl;
			}

			var builder = new StringBuilder(CognitiveServicesUri);

			if (!CognitiveServicesUri.EndsWith("/"))
			{
				builder.Append("/");
			}

			builder.Append(EndpointName);
			builder.Append("/");

			if (!ServiceVersionNumber.StartsWith("v"))
			{
				builder.Append("v");
			}

			builder.Append(ServiceVersionNumber);

			_fullEndpointUrl = builder.ToString();

			return _fullEndpointUrl;
		}
	}
}