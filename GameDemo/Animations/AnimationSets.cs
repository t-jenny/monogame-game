using System;
using System.Collections.Generic;

namespace GameDemo.Animations
{
    public class AnimationSets
    {
        public HashSet<String> NoLoop {
            get
            {
                HashSet<String> Set = new HashSet<String>
                {
                    "sneezing"
                };
                return Set;
            }
        }

        public Dictionary<String, String> CharacterDefault
        {
            get
            {
                Dictionary<String, String> Dictionary = new Dictionary<String, String>
                {
                    { "edgeworth", "standing" }
                };
                return Dictionary;
            }
        }
    }
}
