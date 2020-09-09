using System.Configuration;
using System.Web.Configuration;

namespace Verndale.CognitiveImageTagging
{
	/// <summary>
	/// This has to be here to support the App.Config file modification through code.
	/// </summary>
	public class Configuration : ConfigurationSectionGroup
	{

		#region Constructor
		protected Configuration()
		{

		}
		#endregion

		#region Access Methods

		private static Configuration _current;

		public static Configuration Current
		{
			get
			{
				if (_current == null)
				{
					var config = WebConfigurationManager.OpenWebConfiguration(null);
					_current = config.GetSectionGroup("CognitiveImageTagging") as Configuration;
				}

				return _current;
			}
		}

		#endregion
	}
}
