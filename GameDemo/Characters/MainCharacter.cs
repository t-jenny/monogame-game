using System;
using System.Collections.Generic;

namespace GameDemo.Characters
{
    public class MainCharacter
    {
        Dictionary<string, int> Stats { get; set; }
        Dictionary<string, int> Relationships { get; set; }
        HashSet<string> EventFlags { get; set; }
        HashSet<string> Journal { get; set; }
        HashSet<string> Inventory { get; set; }
    }
}
