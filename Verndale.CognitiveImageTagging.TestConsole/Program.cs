using System;
using System.IO;
using Verndale.CognitiveImageTagging.Services;

namespace Verndale.CognitiveImageTagging.TestConsole
{
	public static class Program
	{
		public static void Main()
		{
			MakeRequest();
			Console.WriteLine("...");
			Console.ReadLine();
		}

		private static async void MakeRequest()
		{
			var imageAnalysis = new AzureService();

			// Get the path and filename to process from the user.
			Console.WriteLine("Analyze an image:");
			Console.Write(
				"Enter the path to the image you wish to analyze:");
			var imageFilePath = System.Console.ReadLine();

			if (string.IsNullOrEmpty(imageFilePath))
			{
				return;
			}

			if (File.Exists(imageFilePath))
			{
				// Call the REST API method.
				Console.WriteLine("\nWait a moment for the results to appear.\n");

				using (Stream stream = File.Open(imageFilePath, FileMode.Open))
				{
					var result = await imageAnalysis.GetImageDescription(stream, "en", true);

					if (result.Status != ImageResult.ResultStatus.Success)
					{
						Console.WriteLine($"Unsuccessful: {result.Status}");

						if (result.Status == ImageResult.ResultStatus.Error)
						{
							Console.WriteLine("Error");
							Console.WriteLine(result.Exception.Message);
						}
					}



					Console.WriteLine("Captions: ");
					foreach (var text in result.Captions)
					{
						Console.WriteLine(text);
					}

					Console.WriteLine("Tags: " + string.Join(",", result.Tags));

					Console.WriteLine($"Contains text? {result.HasEmbeddedText}");

					Console.WriteLine($"Embedded Text: {result.EmbeddedText}");
				}
			}
			else
			{
				Console.WriteLine("Invalid file path");
			}
		}
	}
}
