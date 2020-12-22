using System;
using System.Collections.Generic;

namespace GameDemo.Characters
{
    public class MainCharacter
    {
        public string Name { get; set; }
        public double DayOffset { get; set; } = 0.0;
        public int CurrentCase { get; set; } = 0;
        public Dictionary<string, int> Stats { get; set; }
        public Dictionary<string, int> Relationships { get; set; }
        public HashSet<int> TestimonyIds { get; set; }

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
            TestimonyIds = new HashSet<int>();
        }

        public void NextTimeBlock()
        {
            DayOffset += 0.5;
        }

        public void NextDay()
        {
            DayOffset += 1.0;
        }

        public void ToWeekend()
        {
            DayOffset += 5.0;
        }

        public void ResetTime()
        {
            DayOffset = 0.0;
        }

        public DateTime GetDate()
        {
            DateTime DT = new DateTime(2025, 6, 2, 0, 0, 0);
            return DT.AddDays(DayOffset);
        }

        public string GetDateTimeString()
        {
            DateTime DT = new DateTime(2025, 6, 2, 0, 0, 0);
            DateTime CurrentDate = DT.AddDays(DayOffset);
            string DateString = CurrentDate.ToString("dddd, MMMM dd");
            string TimeOfDay = "Morning";
            if (CurrentDate.Hour > 0) TimeOfDay = "Afternoon";
            return DateString + (": " + TimeOfDay);
        }

    }
}
