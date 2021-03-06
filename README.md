# Verndale Cognitive Image Tagging
A small tool that connects to Azure Cognitive Services to give you descriptive text for image files.

## This library can perform the following:

- Using AI, "read" the image and provide a sentence description of what it portrays.
- Using AI, "read" the image and provide series of tags that describe what it portrays.
- Using AI, perform OCR on the image and, if the image has embedded text, extract the text to a string value.


## Prerequisites
You must have a subscription to Azure Cognitive Services for Computer Vision. You will need both a Subscription Key and an endpoint URL to use this library. Support for other AI
services are not implemented, but you can see where the extension would occur.


## Installation
Install this package using NuGet. 

https://www.nuget.org/packages/Verndale.CognitiveImageTagging/2.0.1.20240

`Install-Package Verndale.CognitiveImageTagging -Version 2.0.1.20240`

If used in a plain old .NET application, Configure your app.config file with appropriate AzureServiceConnections.

If used in a web application, your web.config file will point to a config file in /App_Config/.
You need to copy the example config file provided, remove the ".example" extensions and update the settings
with appropriate AzureServiceConnections.

Note that version 2.0 supports multiple, named Azure connections so you can have
different accounts for different server environments as necessary.

In the supplied config example, you will see confidence levels for captions and embedded text. These are set to reasonable defaults, but be aware the AI can be
quirky and unreliable. This tool should be used prime the pump on image descriptions but should not be used unsupervised.

## Usage
To use Image Tagging, get an instance of IImageTagger through Verndale.CognitiveImageTagging.TagManager.GetImageTagger(nameOfConnection)
The ImageTagger has two methods

IImageTagger.GetImageDescription()
IImageTagger.ExtractTextFromImage()

OCR can be performed simultaneously with GetImageDescription via a parameter flag. Keep in mind you're hitting an external service that not only cost money but
can take some time to complete its evaluation. If you're sending a large batch of images through this utility, expect it to take some time and run up the bill.

Note that OCR tends to be the more expensive operation, both in terms of elapsed time and number of (billed) connections to your AI service.
