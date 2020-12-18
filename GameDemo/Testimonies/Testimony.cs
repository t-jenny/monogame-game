using System;
using System.Collections.Generic;

namespace GameDemo.Testimonies
{
    public class Testimony
    {
        public int Id { get; set; }
        public string CharacterKey { get; set; }
        public string TopicTag { get; set; }
        public bool IsInitial { get; set; }
        public string FrontPadding { get; set; }
        public string Text { get; set; }
        public string EndPadding { get; set; }
        public string SpokenText { get; set; }
        public int IdContradict { get; set; }
    }
}
