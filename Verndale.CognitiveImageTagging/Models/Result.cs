using System.Collections.Generic;

namespace Verndale.CognitiveImageTagging.Models
{
    public class Result
    {
        public ResultStatus Status { get; set; }

        public IList<CaptionResult> Captions { get; set; }

        public IList<string> Tags { get; set; }

        public IList<string> Errors { get; set; }
    }
}
