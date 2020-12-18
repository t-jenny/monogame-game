using System;
using System.Collections.Generic;
namespace GameDemo
{
    public class Case
    {
        public uint Id { get; set; }
        public string Culprit { get; set; }
        public Dictionary<string, string> CharDict { get; set; }
        public HashSet<string> Suspects { get; set; }
        public HashSet<string> TestimonyOnly { get; set; }
        public HashSet<string> Locations { get; set; }
    }
}
