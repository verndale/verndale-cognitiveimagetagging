using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Verndale.CognitiveImageTagging.ImageTaggers.Azure
{
	/// <summary>
	/// Facade around Azure's Cognitive Services Image Processing.
	/// </summary>
	public class AzureImageTagger : IImageTagger
	{
		#region locals

		protected readonly ConnectionDetails ConnectionDetails;
		#endregion

		#region Constructors
		/// <summary>
		/// Creates a new instance of AzureService using the provided Connection Details.
		/// </summary>
		/// <param name="connectionDetails">The AzureConnection details to use.</param>
		public AzureImageTagger(ConnectionDetails connectionDetails)
		{
			ConnectionDetails = connectionDetails;
		}
		#endregion

		#region public Methods
		/// <summary>
		/// Given an Image, use AI to review the image and generate words and sentences to describe the contents of the image in the supplied language. Optionally, specify any text that is embedded within the image itself.
		/// </summary>
		/// <param name="image">The image to process.</param>
		/// <param name="descriptionLanguage">The language for the caption and tags returned.</param>
		/// <param name="checkForTextInImage">Whether to run a check for embedded text. Note that this requires two additional connections to Azure, which may affect your utilization.</param>
		/// <returns>A result object containing a list of tags, descriptions, whether there was embedded text, and what the embedded text was.</returns>
		public async Task<ImageResult> GetImageDescription(Stream image, string descriptionLanguage, bool checkForTextInImage)
		{
			byte[] bytes = new byte[image.Length];
			image.Read(bytes, 0, bytes.Length);
			return await GetImageDescription(bytes, descriptionLanguage, checkForTextInImage);
		}

		/// <summary>
		/// Given an Image, use AI to review the image and generate words and sentences to describe the contents of the image in the supplied language. Optionally, specify any text that is embedded within the image itself.
		/// </summary>
		/// <param name="image">The image to process.</param>
		/// <param name="descriptionLanguage">The language for the caption and tags returned.</param>
		/// <param name="checkForTextInImage">Whether to run a check for embedded text. Note that this requires two additional connections to Azure, which may affect your utilization.</param>
		/// <returns>A result object containing a list of tags, descriptions, whether there was embedded text, and what the embedded text was.</returns>
		public async Task<ImageResult> GetImageDescription(byte[] image, string descriptionLanguage, bool checkForTextInImage)
		{
			try
			{
				var result = new ImageResult();

				var analysis = await MakeDescriptiveAnalysisRequestToAzure(image, descriptionLanguage);

				if (analysis == null)
				{
					// we had some sort of comm failure, but oddly, no exceptions.
					result.Status = ImageResult.ResultStatus.NoResponse;
					return result;
				}


				if (checkForTextInImage && ImageLikelyContainsText(analysis))
				{
					result = await ExtractTextFromImage(image);

					if (result == null)
					{
						// again, some sort of comm failure, but oddly, no exceptions.
						result = new ImageResult { Status = ImageResult.ResultStatus.NoResponse };
						return result;
					}
				}

				result.Captions = GetCaptionsAboveThreshold(analysis);
				result.Tags = analysis.Description.Tags;

				return result;
			}
			catch (Exception ex)
			{
				return new ImageResult { Status = ImageResult.ResultStatus.Error, Exception = ex };
			}
		}

		/// <summary>
		/// Given an image, use AI to review the image and extract any text that is embedded within it. Note that it takes two requests to Azure to fulfill this method.
		/// </summary>
		/// <param name="image">The image to process.</param>
		/// <returns>A result object specifying whether there was embedded text, and what the embedded text was.</returns>
		public async Task<ImageResult> ExtractTextFromImage(Stream image)
		{
			byte[] bytes = new byte[image.Length];
			image.Read(bytes, 0, bytes.Length);
			return await ExtractTextFromImage(bytes);
		}

		/// <summary>
		/// Given an image, use AI to review the image and extract any text that is embedded within it. Note that it takes two requests to Azure to fulfill this method.
		/// </summary>
		/// <param name="image">The image to process.</param>
		/// <returns>A result object specifying whether there was embedded text, and what the embedded text was.</returns>
		public async Task<ImageResult> ExtractTextFromImage(byte[] image)
		{

			try
			{
				var result = new ImageResult();

				var callbackUrl = await MakeOcrAnalysisRequestToAzure(image);

				Thread.Sleep(1000); // need to give the AI time to OCR the text.

				var wordList = await GetOcrAnalysisResultFromAzure(callbackUrl);

				var builder = new StringBuilder();

				foreach (var word in wordList)
				{
					if (builder.Length > 0)
					{
						builder.Append(" ");
					}

					builder.Append(word);
				}

				result.EmbeddedText = builder.ToString();
				return result;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				return new ImageResult { Status = ImageResult.ResultStatus.Error, Exception = ex };
			}

		}
		#endregion

		#region Main work
		/// <summary>
		/// Submits an image for descriptive analysis. 
		/// </summary>
		/// <param name="imageByteArray">The image to analyze as a byte array.</param>
		/// <param name="language">The language to use in the returned description.</param>
		/// <returns>An Azure ImageAnalysis object or null.</returns>
		private async Task<ImageAnalysis> MakeDescriptiveAnalysisRequestToAzure(byte[] imageByteArray, string language)
		{
			using (var client = new HttpClient())
			{
				client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", ConnectionDetails.SubscriptionKey);
				using (var content = new ByteArrayContent(imageByteArray))
				{
					try
					{
						var uri = ConnectionDetails.GetAnalyzeUrl(language);
						content.Headers.ContentType =
							new MediaTypeHeaderValue("application/octet-stream");

						var response = await client.PostAsync(uri, content);
						response.EnsureSuccessStatusCode();


						var json = await response.Content.ReadAsStringAsync();
						return JsonConvert.DeserializeObject<ImageAnalysis>(json);

					}
					catch (HttpRequestException ex)
					{
						Console.WriteLine(ex);
						throw;
					}

				}
			}
		}


		/// <summary>
		/// Submits an image for OCR analysis. The analysis is async, so a second call needs to be made to retrieve the results.
		/// </summary>
		/// <param name="imageAsBytes">the image to process, as a byte array</param>
		/// <returns>url where the result of analysis can be found. or null.</returns>
		/// <remarks>
		/// Method requires a Response 202 from initial image response
		/// The service has accepted the request and will start processing later.
		/// It will return Accepted immediately and include an "Operation-Location" header.
		/// Client side should further query the read operation status using the URL specified in this header.
		/// The operation ID will expire in 48 hours.
		/// </remarks>
		private async Task<string> MakeOcrAnalysisRequestToAzure(byte[] imageAsBytes)
		{
			using (var client = new HttpClient())
			{
				try
				{
					client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", ConnectionDetails.SubscriptionKey);
					var url = ConnectionDetails.GetOcrUrl();

					HttpResponseMessage response;
					using (ByteArrayContent content = new ByteArrayContent(imageAsBytes))
					{
						content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
						response = await client.PostAsync(url, content);
					}

					// Operation-Location is the URI where the results can be found. There will only be 1 value here, but .NET doesn't let us do this quickly.
					if (response.Headers.TryGetValues("Operation-Location", out var headerValues))
					{
						return headerValues.FirstOrDefault();
					}
				}
				catch (HttpRequestException ex)
				{
					Console.WriteLine(ex);
					throw;
				}

			}

			return null;
		}

		/// <summary>
		/// Calls the supplied URL to get the results of OCR image analysis.
		/// </summary>
		/// <param name="resultEndpoint">the URL where the results can be found.</param>
		/// <returns>The words that were found embedded within the image.</returns>
		private async Task<IEnumerable<string>> GetOcrAnalysisResultFromAzure(string resultEndpoint)
		{
			if (string.IsNullOrEmpty(resultEndpoint))
			{
				return null;
			}

			try
			{
				using (var client = new HttpClient())
				{
					client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", ConnectionDetails.SubscriptionKey);
					var response = await client.GetAsync(resultEndpoint);
					if (response == null)
					{
						return null;
					}

					var json = await response.Content.ReadAsStringAsync();
					var result = JsonConvert.DeserializeObject<TextOperationResult>(json);

					if (result.Status != TextOperationStatusCodes.Succeeded)
					{
						return null;
					}

					return result.RecognitionResult.Lines.Select(line => line.Text);
				}
			}
			catch (HttpRequestException ex)
			{
				Console.WriteLine(ex);
				throw;
			}
		}
		#endregion

		#region Helpers
		/// <summary>
		/// Reviews the Azure analysis and includes any Caption text that exceeds the current Caption Confidence Level setting.
		/// Captions are sorted by highest confidence level.
		/// </summary>
		/// <param name="analysis">The Azure ImageAnalysis object to interrogate.</param>
		/// <returns>a list of captions sorted by confidence level.</returns>
		private IEnumerable<string> GetCaptionsAboveThreshold(ImageAnalysis analysis)
		{
			var confidenceThreshold = ConnectionDetails.CaptionConfidenceLevel;

			var captions = new SortedList<double, string>();

			foreach (var caption in analysis.Description.Captions)
			{
				if (caption.Confidence >= confidenceThreshold)
				{
					captions.Add(caption.Confidence, caption.Text);
				}
			}

			return captions.Values.Reverse();
		}


		/// <summary>
		/// Check if the initial analysis returns an indication that the image has text in it;
		/// if so, check the confidence level of the image having text.
		/// </summary>
		/// <param name="imageAnalysis">object returned from Cognitive Services after image analysis</param>
		/// <returns>true, if image contains text within the confidence threshold. otherwise, false</returns>
		private bool ImageLikelyContainsText(ImageAnalysis imageAnalysis)
		{
			var confidenceThreshold = ConnectionDetails.TextInImageConfidenceLevel;

			foreach (var tag in imageAnalysis.Tags)
			{
				if (tag.Name != "text")
				{
					continue;
				}

				if (tag.Confidence >= confidenceThreshold)
				{
					return true;
				}
			}

			return false;
		}
		#endregion
	}
}
