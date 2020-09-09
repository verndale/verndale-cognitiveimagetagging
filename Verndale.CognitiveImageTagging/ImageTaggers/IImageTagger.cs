using System.IO;
using System.Threading.Tasks;

namespace Verndale.CognitiveImageTagging.ImageTaggers
{
	public interface IImageTagger
	{
		/// <summary>
		/// Given an Image, use AI to review the image and generate words and sentences to describe the contents of the image in the supplied language. Optionally, specify any text that is embedded within the image itself.
		/// </summary>
		/// <param name="image">The image to process.</param>
		/// <param name="descriptionLanguage">The language for the caption and tags returned.</param>
		/// <param name="checkForTextInImage">Whether to run a check for embedded text. Note that this calls ExtractTextFromImage(), which will trigger additional calls to your AI service.</param>
		/// <returns>A result object containing a list of tags, descriptions, whether there was embedded text, and what the embedded text was.</returns>
		Task<ImageResult> GetImageDescription(Stream image, string descriptionLanguage, bool checkForTextInImage);

		/// <summary>
		/// Given an Image, use AI to review the image and generate words and sentences to describe the contents of the image in the supplied language. Optionally, specify any text that is embedded within the image itself.
		/// </summary>
		/// <param name="image">The image to process.</param>
		/// <param name="descriptionLanguage">The language for the caption and tags returned.</param>
		/// <param name="checkForTextInImage">Whether to run a check for embedded text. Note that this requires two additional connections to Azure, which may affect your utilization.</param>
		/// <returns>A result object containing a list of tags, descriptions, whether there was embedded text, and what the embedded text was.</returns>
		Task<ImageResult> GetImageDescription(byte[] image, string descriptionLanguage, bool checkForTextInImage);

		/// <summary>
		/// Given an image, use OCR or equivalent AI to extract any text within the image.
		/// </summary>
		/// <param name="image">The image to process.</param>
		/// <returns>A result object specifying whether there was embedded text, and what the embedded text was.</returns>
		Task<ImageResult> ExtractTextFromImage(Stream image);

		/// <summary>
		/// Given an image, use OCR or equivalent AI to extract any text within the image.
		/// </summary>
		/// <param name="image">The image to process.</param>
		/// <returns>A result object specifying whether there was embedded text, and what the embedded text was.</returns>
		Task<ImageResult> ExtractTextFromImage(byte[] image);
	}
}
