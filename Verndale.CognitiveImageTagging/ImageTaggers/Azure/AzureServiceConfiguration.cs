using System.Configuration;

namespace Verndale.CognitiveImageTagging.ImageTaggers.Azure
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
		/// The Default Azure Connection information.
		/// </summary>
		public ConnectionDetails DefaultConnection
		{
			get
			{
				if (string.IsNullOrEmpty(this.DefaultConnectionName))
				{
					return Connections[0];
				}

				return Connections[this.DefaultConnectionName];
			}
		}

		/// <summary>
		/// The connection name to use if none is supplied.
		/// </summary>
		[ConfigurationProperty("defaultConnectionName", IsRequired = true)]
		public string DefaultConnectionName
		{
			get => this["defaultConnectionName"].ToString();
			set => this["defaultConnectionName"] = value;
		}

		/// <summary>
		/// All defined Azure Connections.
		/// </summary>
		[ConfigurationProperty("connections", IsDefaultCollection = false)]
		[ConfigurationCollection(typeof(ConnectionCollection),
			AddItemName = "add",
			ClearItemsName = "clear",
			RemoveItemName = "remove")]
		public ConnectionCollection Connections
		{
			get
			{
				return (ConnectionCollection)base["connections"];
			}
		}
	}
}