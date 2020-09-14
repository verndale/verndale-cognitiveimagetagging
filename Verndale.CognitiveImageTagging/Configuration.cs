using System.Configuration;
using Verndale.CognitiveImageTagging.ImageTaggers.Azure;

namespace Verndale.CognitiveImageTagging
{
	/// <summary>
	/// This has to be here to support the App.Config file modification through code.
	/// </summary>
	public class Configuration : ConfigurationSection
	{
		public static Configuration Current
		{
			get
			{
				return ConfigurationManager.GetSection("cognitiveImageTagging") as Configuration;
			}
		}

		[ConfigurationProperty("azureService")]
		public AzureServiceConfiguration AzureService
		{
			get
			{
				AzureServiceConfiguration azureService = (AzureServiceConfiguration)base["azureService"];

				return azureService;
			}
		}

	}
}
