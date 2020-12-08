using System;
using System.Collections.Generic;

namespace GameDemo.Characters
{
    public class MainCharacter
    {
        public string Name = "DJ Tang-izzle";
        public double DayOffset;
        public Dictionary<string, int> Stats { get; set; }
        public Dictionary<string, int> Relationships { get; set; }
        public HashSet<string> EventFlags { get; set; }
        public HashSet<string> Journal { get; set; }
        public HashSet<string> Inventory { get; set; }

        public MainCharacter()
        {
            DayOffset = 0.0;
            Stats = new Dictionary<string, int>();
            Relationships = new Dictionary<string, int>();
            EventFlags = new HashSet<string>();
            Journal = new HashSet<string>();
            Inventory = new HashSet<string>();
        }

        public void NextDay()
        {
            DayOffset += 1.0;
        }

        public DateTime GetDate()
        {
            DateTime DT = new DateTime(2025, 5, 31);
            return DT.AddDays(DayOffset);
        }

    }
}
