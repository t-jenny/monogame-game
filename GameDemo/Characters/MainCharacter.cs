using System;
using System.Collections.Generic;

namespace GameDemo.Characters
{
    public class MainCharacter
    {
        public Dictionary<string, int> Stats { get; set; }
        public Dictionary<string, int> Relationships { get; set; }
        public HashSet<string> EventFlags { get; set; }
        public HashSet<string> Journal { get; set; }
        public HashSet<string> Inventory { get; set; }

        public MainCharacter()
        {
            Stats = new Dictionary<string, int>();
            Relationships = new Dictionary<string, int>();
            EventFlags = new HashSet<string>();
            Journal = new HashSet<string>();
            Inventory = new HashSet<string>();
        }
    }
}
