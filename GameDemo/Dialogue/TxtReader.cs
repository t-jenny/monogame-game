using System;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework.Content;

namespace GameDemo.Dialogue
{
    public class TxtReader
    {
        private const String CHARACTER = "@"; //@1 and @2 means there are two characters on screen
        private const String ANIMATION = "#";
        private const String MUSIC_OVERRIDE = "%";
        private String path;
        string[] text;
        int dialogueIndex;

        public TxtReader(ContentManager content)
        {
            path = Path.Combine(content.RootDirectory, "sample-story.txt");
            text = File.ReadAllLines(path, Encoding.UTF8);
            dialogueIndex = 0;
        }

        public void nextLine()
        {
            dialogueIndex++;
        }

        public String currentLine()
        {
            return text[dialogueIndex];
        }

        public String getLocation()
        {
            return text[dialogueIndex].Substring(2);
        }

        public String[] getDialogue()
        {
            String currentString = text[dialogueIndex];
            int characterIndex = currentString.IndexOf(CHARACTER);
            int animationIndex = currentString.IndexOf(ANIMATION);

            String[] dialogue = new string[] {
                currentString.Substring(0, characterIndex),
                currentString.Substring(characterIndex + 1, animationIndex - characterIndex - 2),
                currentString.Substring(animationIndex + 1)
                };

            return dialogue;
        }
    }
}