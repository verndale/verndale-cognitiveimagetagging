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
			// TODO: make this configurable once we've got more than one.
			return new AzureImageTagger(AzureServiceConfiguration.Current.DefaultConnection);
		}

		public static IImageTagger GetImageTagger(string connectionName)
		{
			return new AzureImageTagger(AzureServiceConfiguration.Current.Connections[connectionName]);
		}
	}
}
