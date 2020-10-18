using System.Collections.Generic;
using System.Xml.Serialization;

namespace GameDemo.Dialogue
{
    public class DialogueConditions
    {
        public Dictionary<string, int> RequiredStats { get; set; }

        public Dictionary<string, int> RequiredRelationships { get; set; }

        public HashSet<string> RequiredEventFlags { get; set; }

        public string EventText { get; set; }
    }
}
