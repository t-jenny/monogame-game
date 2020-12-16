using System;
using System.Collections.Generic;


namespace GameDemo.Characters
{
    public class Character
    {
        public string Name { get; set; }
        public string Occupation { get; set; }
        public string ImagePath { get; set; }
        public uint Age { get; set; }
        public string Description { get; set; }
        public string Personality { get; set; }
        public List<string> BFFs { get; set; }
        public List<string> Greetings { get; set; }
    }
}
