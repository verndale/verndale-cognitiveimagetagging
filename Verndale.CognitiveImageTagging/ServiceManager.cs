using Verndale.CognitiveImageTagging.Services;

namespace Verndale.CognitiveImageTagging
{
	/// <summary>
	/// Provides access to the currently configured Analysis Service. The active service is specified in the Config file.
	/// </summary>
	public class ServiceManager
	{
		/// <summary>
		/// Returns the currently configured AnalysisService
		/// </summary>
		/// <returns>An instance of IAnalysisService</returns>
		public static IAnalysisService GetAnalysisService()
		{
			// TODO: make this configurable once we've got more than one.
			return new AzureService();
		}
	}
}
