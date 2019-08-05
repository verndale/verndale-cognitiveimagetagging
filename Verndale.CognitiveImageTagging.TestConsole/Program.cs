using System;
using System.IO;

namespace Verndale.CognitiveImageTagging.TestConsole
{
	public static class Program
	{
		public static void Main()
		{
			MakeRequest();
			Console.WriteLine("[enter] to end program.");
			Console.ReadLine();
		}

		private static async void MakeRequest()
		{
			var path = GetFilePath();

			if (!string.IsNullOrEmpty(path) && File.Exists(path))
			{
				using (Stream stream = File.Open(path, FileMode.Open))
				{
					var service = ServiceManager.GetAnalysisService();

					var result = await service.GetImageDescription(stream, "en", true);

					if (result.Status != ImageResult.ResultStatus.Success)
					{
						RenderError(result);
					}
					else
					{
						RenderOutput(result);
					}
				}
			}
			else
			{
				Console.WriteLine("Invalid file path");
			}
		}

		private static string GetFilePath()
		{
			Console.Write("Enter the local path to the image you wish to analyze:");
			return Console.ReadLine();
		}

		private static void RenderError(ImageResult result)
		{
			Console.WriteLine($"Unsuccessful: {result.Status}");

			if (result.Status == ImageResult.ResultStatus.Error)
			{
				Console.WriteLine("Exception: ");
				Console.WriteLine(result.Exception.Message);
			}
		}

		private static void RenderOutput(ImageResult result)
		{
			Console.WriteLine("Captions: ");
			foreach (var text in result.Captions)
			{
				Console.WriteLine(text);
			}

			Console.WriteLine($"Tags: {string.Join(", ", result.Tags)}");

			if (result.HasEmbeddedText)
			{
				Console.WriteLine($"Embedded Text: {result.EmbeddedText}");
			}
		}
	}
}
