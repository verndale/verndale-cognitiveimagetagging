using System;
using System.Collections.Generic;

namespace Verndale.CognitiveImageTagging
{
	public class ImageResult
	{
		public enum ResultStatus
		{
			Success,
			NoResponse,
			Error
		}

		public ImageResult()
		{
			Tags = new List<string>();
			Captions = new List<string>();
			Status = ResultStatus.Success;
		}

		public IEnumerable<string> Tags { get; set; }

		public IEnumerable<string> Captions { get; set; }

		public bool HasEmbeddedText
		{
			get { return !string.IsNullOrEmpty(EmbeddedText); }
		}

		public string EmbeddedText { get; set; }

		public ResultStatus Status { get; set; }

		public Exception Exception { get; set; }
	}
}
