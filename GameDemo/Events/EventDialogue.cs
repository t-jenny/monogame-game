using System.Collections.Generic;

namespace GameDemo.Events
{
    public class EventDialogue
    {
        public string EventName { get; set; }

        public Dictionary<string, int> RequiredStats { get; set; }

        public Dictionary<string, int> RequiredRelationships { get; set; }

        public HashSet<string> RequiredFlags { get; set; }

        public string Text { get; set; }
    }
}
