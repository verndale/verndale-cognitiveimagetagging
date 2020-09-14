using System.Configuration;
using Verndale.CognitiveImageTagging.ImageTaggers;
using Verndale.CognitiveImageTagging.ImageTaggers.Azure;

namespace Verndale.CognitiveImageTagging
{
	/// <summary>
	/// Provides access to the currently configured Analysis Service. The active service is specified in the Config file.
	/// </summary>
	public class ImageTaggingManager
	{
		/// <summary>
		/// Returns the currently configured AnalysisService
		/// </summary>
		/// <returns>An instance of IAnalysisService</returns>
		public static IImageTagger GetImageTagger()
		{
			return GetImageTagger(string.Empty);
		}

		public static IImageTagger GetImageTagger(string connectionName)
		{
			if (Configuration.Current == null)
			{
				throw new ConfigurationErrorsException("Verndale.CognitiveImageTagging: Missing configuration element \"cognitiveImageTagging\".");
			}

			if (Configuration.Current.AzureService == null)
			{
				throw new ConfigurationErrorsException("Verndale.CognitiveImageTagging: Missing configuration element \"cognitiveImageTagging\\azureService\".");
			}

			ConnectionDetails connectionDetails;

			if (string.IsNullOrEmpty(connectionName))
			{
				connectionDetails = Configuration.Current.AzureService.DefaultConnection;
			}
			else
			{
				connectionDetails = Configuration.Current?.AzureService?.Connections[connectionName];
			}

			if (connectionDetails == null)
			{
				throw new ConfigurationErrorsException($"Verndale.CognitiveImageTagging: Missing default or named ConnectionDetails.");
			}

			return new AzureImageTagger(connectionDetails);
		}
	}
}