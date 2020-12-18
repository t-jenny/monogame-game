using System;
using System.Collections.Generic;

namespace GameDemo.Testimonies
{
    public class TestimonyList
    {
        // Add Case Id to the list
        public Dictionary<string, string> Topics { get; set; }
        public List<Testimony> Testimonies { get; set; }
    }
}
