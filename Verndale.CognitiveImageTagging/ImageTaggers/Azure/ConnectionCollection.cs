using System.Configuration;

namespace Verndale.CognitiveImageTagging.ImageTaggers.Azure
{
	/// <summary>
	/// A collection of Azure Connection details from the app config
	/// </summary>
	public class ConnectionCollection : ConfigurationElementCollection
	{
		/// <inheritdoc />
		public ConnectionCollection()
		{

		}

		public ConnectionDetails this[int index]
		{
			get { return (ConnectionDetails)BaseGet(index); }
			set
			{
				if (BaseGet(index) != null)
				{
					BaseRemoveAt(index);
				}
				BaseAdd(index, value);
			}
		}

		/// <summary>
		/// Gets the Azure Connection details by their name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public new ConnectionDetails this[string name]
		{
			get
			{
				return (ConnectionDetails)BaseGet(name);
			}
		}

		public void Add(ConnectionDetails azureConnection)
		{
			BaseAdd(azureConnection);
		}

		public void Clear()
		{
			BaseClear();
		}

		protected override ConfigurationElement CreateNewElement()
		{
			return new ConnectionDetails();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((ConnectionDetails)element).Name;
		}

		public void Remove(ConnectionDetails azureConnection)
		{
			BaseRemove(azureConnection.Name);
		}

		public void RemoveAt(int index)
		{
			BaseRemoveAt(index);
		}

		public void Remove(string name)
		{
			BaseRemove(name);
		}
	}
}
