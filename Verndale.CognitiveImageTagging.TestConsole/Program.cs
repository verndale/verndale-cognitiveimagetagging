using System.IO;
using Verndale.CognitiveImageTagging.Services.ComputerVision;

namespace Verndale.CognitiveImageTagging.TestConsole
{
    public static class Program
    {
        public static void Main()
        {
            MakeRequest();
            System.Console.WriteLine("...");
            System.Console.ReadLine();
        }

        static async void MakeRequest()
        {
            var imageAnalysis = new ImageAnalyzer();

            // Get the path and filename to process from the user.
            System.Console.WriteLine("Analyze an image:");
            System.Console.Write(
                "Enter the path to the image you wish to analyze:");
            string imageFilePath = System.Console.ReadLine();

            if (File.Exists(imageFilePath))
            {
                // Call the REST API method.
                System.Console.WriteLine("\nWait a moment for the results to appear.\n");

                if (imageFilePath == null)
                {
                    System.Console.WriteLine("\nImage is not available.\n");
                    return;
                }

                using (Stream stream = File.Open(imageFilePath, FileMode.Open))
                {
                    var metadata = await imageAnalysis.GetAltTextAndTags(stream, "en", true);

                    System.Console.WriteLine("\nCaptions: ");
                    foreach (var text in metadata.Captions)
                    {
                        System.Console.WriteLine(text.Caption);
                    }

                    System.Console.WriteLine("\nTags: " + string.Join(",", metadata.Tags));

                    System.Console.WriteLine("\n\nPress enter to exit");
                }
            }
            else
            {
                System.Console.WriteLine("\nInvalid file path");
            }
        }
    }
}
