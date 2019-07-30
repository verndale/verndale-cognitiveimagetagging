using System.Collections.Generic;

namespace Verndale.CognitiveImageTagging.Models
{
    public class CsImageMetadata
    {
        public IList<string> Captions { get; set; }

        public IList<string> Tags { get; set; }
    }
}
