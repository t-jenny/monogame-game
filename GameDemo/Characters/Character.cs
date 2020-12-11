using System;
using System.Collections.Generic;

namespace GameDemo.Characters
{
    public class AllCharacters
    {
        public Dictionary<String, Character> AllChars { get; set; }
    }

    public class Character
    {
        public string Name { get; set; }
        public string ImagePath { get; set; }
        public uint Age { get; set; }
        public string Description { get; set; }
        public string Personality { get; set; }
        public List<String> BFFs { get; set; }
    }
}
