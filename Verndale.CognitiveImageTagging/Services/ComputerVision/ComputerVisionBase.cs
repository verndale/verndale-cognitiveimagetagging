using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System;
using System.IO;
using System.Net.Http;

namespace Verndale.CognitiveImageTagging.Services.ComputerVision
{
	public abstract class ComputerVisionBase
	{
		protected readonly HttpClient Client;

		protected readonly string SubscriptionKey;
		protected readonly string Endpoint;

		#region ctor
		protected ComputerVisionBase(HttpClient client)
		{
			string endpoint = Configuration.Current.CognitiveServicesUri;
			string subscriptionKey = Configuration.Current.SubscriptionKey;

			if (string.IsNullOrWhiteSpace(endpoint))
			{
				throw new ComputerVisionErrorException("CognitiveServicesUri must be defined in configuration.");
			}

			if (string.IsNullOrWhiteSpace(subscriptionKey))
			{
				throw new ComputerVisionErrorException("Subscription must be defined in configuration");
			}

			SubscriptionKey = subscriptionKey;
			Endpoint = endpoint;
			Client = client;
		}
		#endregion

		internal static byte[] GetImageByteData(Stream imageStream)
		{
			Byte[] byteData = new Byte[imageStream.Length];
			imageStream.Read(byteData, 0, byteData.Length);
			return byteData;
		}
	}
}