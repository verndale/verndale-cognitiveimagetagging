using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using Verndale.CognitiveImageTagging.Models;

namespace Verndale.CognitiveImageTagging.Services.ComputerVision
{
	public class ImageAnalyzer : ComputerVisionBase
	{
		private readonly double _textInImageConfidenceLevel;
		private readonly double _captionConfidenceLevel;

		#region ctor
		/// <summary>
		/// Instantiate a new image analyzer to retrieve tags and captions for an image.
		/// </summary>
		public ImageAnalyzer()
			: this(new HttpClient()) { }

		/// <summary>
		/// Instantiate a new image analyzer to retrieve tags and captions for an image.
		/// </summary>
		/// <param name="client">HttpClient</param>
		public ImageAnalyzer(HttpClient client)
			: base(client)
		{
			_textInImageConfidenceLevel = Configuration.Current.TextInImageConfidenceLevel;
			_captionConfidenceLevel = Configuration.Current.CaptionConfidenceLevel;
		}
		#endregion

		/// <summary>
		/// The <param name="imageStream"></param> is sent to Azure Cognitive Services to identify list of possible captions.   The first caption has the highest level of confidence.
		/// </summary>
		/// <param name="imageStream">image stream. <code>Sitecore: (MediaItem).GetMediaStream()</code> <code>Episerver: (MediaFile).BinaryData.OpenRead()</code>.
		/// Supported image formats: JPEG, PNG, GIF, BMP.
		/// Image file size must be less than 4MB.
		/// Image dimensions should be greater than 50 x 50.
		/// </param>
		/// <param name="language">See https://docs.microsoft.com/en-us/azure/cognitive-services/computer-vision/language-support</param>. NOTE:  Only 'en' is supported when recognizing text in images.
		/// <param name="checkForTextInImage">If an image has text inside it, it requires two separate calls to Cognitive Services.  If this is set to true, a call will be made to identify shapes in the image.
		/// A second call is then made to identify the text in the image.  The two values are concatenated together.</param>
		/// <returns>List of possible captions in order of confidence, descending and a list of possible tags</returns>
		public async Task<CsImageMetadata> GetAltTextAndTags(Stream imageStream, string language = "en", bool checkForTextInImage = false)
		{
			var altTextList = new List<string>();

			var imageAnalysis = await AnalyzeImage(imageStream, language, checkForTextInImage);

			if (imageAnalysis.Description?.Captions == null)
			{
				throw new ComputerVisionErrorException("No tags or caption was returned for the image; Description/Caption is not present in the returned object.");
			}

			var index = 0;
			foreach (var caption in imageAnalysis.Description.Captions.OrderByDescending(x => x.Confidence))
			{
				if (index == 0)
				{
					altTextList.Add(caption.Text);
					++index;
					continue;
				}
				++index;
				if (caption.Confidence > _captionConfidenceLevel)
					altTextList.Add(caption.Text);
			}

			return new CsImageMetadata
			{
				Captions = altTextList,
				Tags = imageAnalysis.Description.Tags
			};
		}

		/// <summary>
		/// Receive complete image analysis object, including confidence levels.
		/// </summary>
		/// <param name="imageStream">image stream. <code>Sitecore: (MediaItem).GetMediaStream()</code> <code>Episerver: (MediaFile).BinaryData.OpenRead()</code>.
		/// Supported image formats: JPEG, PNG, GIF, BMP.
		/// Image file size must be less than 4MB.
		/// Image dimensions should be greater than 50 x 50.
		/// </param>
		/// <param name="language">See https://westus.dev.cognitive.microsoft.com/docs/services/5adf991815e1060e6355ad44/operations/56f91f2e778daf14a499e1fa. NOTE:  Only 'en' is supported when recognizing text in images.
		/// </param>
		/// <param name="checkForTextInImage">If an image has text inside it, it requires two separate calls to Cognitive Services.  If this is set to true, a call will be made to identify shapes in the image.
		/// A second call is then made to identify the text in the image.  The two values are concatenated together as comma-separated tags.</param>
		/// <returns><code>ImageAnalysis</code></returns>
		public async Task<ImageAnalysis> GetImageAnalysisJson(Stream imageStream, string language,
			bool checkForTextInImage)
		{
			return await AnalyzeImage(imageStream, language, checkForTextInImage);
		}

		private async Task<ImageAnalysis> AnalyzeImage(Stream imageStream, string language, bool checkForTextInImage)
		{
			// Request headers.
			Client.DefaultRequestHeaders.Add(
				"Ocp-Apim-Subscription-Key", SubscriptionKey);

			var byteData = GetImageByteData(imageStream);

			var imageAnalysis = await IdentifyImageObjects(byteData, language);

			if (checkForTextInImage && ImageLikelyContainsText(imageAnalysis))
			{
				var operationLocationResponse = await GetOperationLocationHeaderForImage(byteData);

				Thread.Sleep(1000);

				IEnumerable<string> operationLocation;

				var result = operationLocationResponse.Headers.TryGetValues("Operation-Location", out operationLocation);

				var wordList = await GetWordListFromImage(operationLocation, result);

				if (wordList.Any())
				{
					imageAnalysis.Description.Tags.Add(string.Join(",", wordList));
					imageAnalysis.Description.Captions.Insert(0, new ImageCaption(string.Join(" ", wordList), 0.96));
				}
			}
			return imageAnalysis;
		}

		private async Task<List<string>> GetWordListFromImage(IEnumerable<string> operationLocation, bool result)
		{
			var headerList = operationLocation.ToList();

			var wordList = new List<string>();

			if (!result || !headerList.ToList().Any()) return wordList;

			var textResponse = await Client.GetAsync(headerList.First());

			var textResponseString = await textResponse.Content.ReadAsStringAsync();

			if (string.IsNullOrWhiteSpace(textResponseString))
			{
				throw new ComputerVisionErrorException("No words were found in the image.");
			}

			var json2 = JToken.Parse(textResponseString).ToString();

			var textOperationResult = new JavaScriptSerializer().Deserialize<TextOperationResult>(json2);

			if (textOperationResult.Status == TextOperationStatusCodes.Succeeded)
			{
				wordList.AddRange(textOperationResult.RecognitionResult.Lines.Select(line => line.Text));
			}

			return wordList;
		}

		/// <summary>
		/// Method requires a Response 202 from initial image response
		/// The service has accepted the request and will start processing later.
		/// It will return Accepted immediately and include an "Operation-Location" header.
		/// Client side should further query the read operation status using the URL specified in this header.
		/// The operation ID will expire in 48 hours.
		/// </summary>
		/// <param name="imageAsBytes">image byte array</param>
		/// <returns>response containing url in operation location header</returns>
		private async Task<HttpResponseMessage> GetOperationLocationHeaderForImage(byte[] imageAsBytes)
		{
			HttpResponseMessage opLocationResponse;

			var queryString = HttpUtility.ParseQueryString(string.Empty);

			queryString["mode"] = Configuration.Current.TextCheckMode;

			// Assemble the URI for the REST API method.
			var textInImageUrl = Endpoint + "/recognizeText?" + queryString;

			using (ByteArrayContent content = new ByteArrayContent(imageAsBytes))
			{
				content.Headers.ContentType =
					new MediaTypeHeaderValue("application/octet-stream");

				//get Operation-Location header
				opLocationResponse = await Client.PostAsync(textInImageUrl, content);
			}

			return opLocationResponse;
		}

		private async Task<ImageAnalysis> IdentifyImageObjects(byte[] byteData, string language)
		{
			var visualFeatures = Configuration.Current.VisualFeatures;
			var details = Configuration.Current.ImageDetails;
			var detectOrientation = Configuration.Current.DetectOrientation;

			string requestParameters =
				$"visualFeatures={visualFeatures}&details={details}&language={language}&detectOrientation={detectOrientation}";

			// Assemble the URI for the REST API method.
			string uri = Endpoint + "/analyze?" + requestParameters;

			HttpResponseMessage response;
			using (ByteArrayContent content = new ByteArrayContent(byteData))
			{
				content.Headers.ContentType =
					new MediaTypeHeaderValue("application/octet-stream");

				response = await Client.PostAsync(uri, content);
			}

			var contentString = await response.Content.ReadAsStringAsync();

			if (string.IsNullOrWhiteSpace(contentString))
			{
				throw new ComputerVisionErrorException("Image Analysis Error:  No data present in response.");
			}

			var json = JToken.Parse(contentString).ToString();

			return new JavaScriptSerializer().Deserialize<ImageAnalysis>(json);
		}

		/// <summary>
		/// Check if the initial analysis returns an indication that the image has text in it;
		/// if so, check the confidence level of the image having text.
		/// </summary>
		/// <param name="imageAnalysis">object returned from Cognitive Services after image analysis</param>
		/// <returns>true, if image contains text within the confidence threshold. otherwise, false</returns>
		private bool ImageLikelyContainsText(ImageAnalysis imageAnalysis)
		{
			var highConfidenceText = false;

			if (imageAnalysis.Tags.Any())
			{
				highConfidenceText = imageAnalysis.Tags.Any(tag => (tag.Name == "text") && (tag.Confidence > _textInImageConfidenceLevel));
			}

			return highConfidenceText;
		}
	}
}