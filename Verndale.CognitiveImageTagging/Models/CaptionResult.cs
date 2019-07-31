using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Verndale.CognitiveImageTagging.Models
{
    public class CaptionResult
    {
        public string Caption { get; set; }

        public bool IsTextFromImage { get; set; }
    }
}
