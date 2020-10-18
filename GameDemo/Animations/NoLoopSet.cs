using System;
using System.Collections.Generic;

namespace GameDemo.Animations
{
    public class NoLoopSet
    {
        public HashSet<String> animations {
            get
            {
                HashSet<String> set = new HashSet<String>
                {
                    "sneezing"
                };
                return set;
            }
        }
    }
}
