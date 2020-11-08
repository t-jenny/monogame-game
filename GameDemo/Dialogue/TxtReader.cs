using System;
using System.Collections.Generic;
using GameDemo.Animations;
using GameDemo.Characters;
using GameDemo.Effects;
using GameDemo.Locations;
using Microsoft.Xna.Framework.Content;

namespace GameDemo.Dialogue
{
    public class TxtReader
    {
        private const int FIRST_INDEX = 0;
        private const int SECOND_INDEX = 1;
        private const int CHANGE_INDEX = 4;

        private const char BACKGROUND = '*';
        private const char AUDIO = '%';
        private const char CHARACTER = '@';
        private const char ANIMATION = '#';
        private const char CHOICE = '>';
        private const char JUMP = '=';
        private const char ADD = '+';
        private const char FLAG = 'f';
        private const char RELATIONSHIP = 'r';
        private const char STAT = 's';
        private const char NOTEBOOK = 'n';
        private const char SPLIT = '/';
        private const char ADDSPLIT = ' ';

        MainCharacter MainCharacter;
        ContentManager Content;
        String[] Text;
        int TextIndex;

        private TxtReader(MainCharacter mainCharacter, ContentManager content)
        {
            this.MainCharacter = mainCharacter;
            this.Content = content;
        }

        public TxtReader(MainCharacter mainCharacter, ContentManager content, String text)
            : this(mainCharacter, content)
        {
            Text = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        }

        public TxtReader(MainCharacter mainCharacter, ContentManager content, String[] text)
            : this(mainCharacter, content)
        {
            Text = text;
        }

        public void NextTxtObject()
        {
            TextIndex++;
        }

        public ITextObject CurrentTxtObject()
        {
            String CurrentString = Text[TextIndex];
            char FirstChar = CurrentString[FIRST_INDEX];
            ITextObject TextObject = null;

            while (TextIndex < Text.Length)
            {
                switch (FirstChar)
                {
                    case BACKGROUND:
                        return new Background(Content, CurrentString.Substring(SECOND_INDEX));

                    case AUDIO:
                        return new Audio(Content, CurrentString.Substring(SECOND_INDEX));

                    case CHARACTER:
                        return new CharacterAnimation(Content, CurrentString.Substring(SECOND_INDEX), null);

                    case CHOICE:
                        List<String[]> ChoiceKeys = new List<String[]>();
                        while (FirstChar == CHOICE)
                        {
                            ChoiceKeys.Add(CurrentString.Substring(SECOND_INDEX).Split(SPLIT));
                            TextIndex++;
                            CurrentString = Text[TextIndex];
                            FirstChar = CurrentString[FIRST_INDEX];
                        }

                        List<String[]> ChoiceValues = new List<String[]>();

                        for (int i = 1; i < ChoiceKeys.Count; i++)
                        {
                            int LastIndex = Array.IndexOf(Text, JUMP + ChoiceKeys[i][FIRST_INDEX]);
                            String[] ChoiceText = new ArraySegment<String>(Text, TextIndex, LastIndex - TextIndex).ToArray();
                            ChoiceValues.Add(ChoiceText);
                            TextIndex = LastIndex + 1;
                        }

                        ChoiceValues.Add(new ArraySegment<String>(Text, TextIndex, Text.Length - TextIndex).ToArray());

                        Dictionary<String[], String[]> ChoiceDictionary = new Dictionary<string[], string[]>();

                        for (int i = 0; i < ChoiceKeys.Count; i++)
                        {
                            ChoiceDictionary.Add(ChoiceKeys[i], ChoiceValues[i]);
                        }

                        return new ChoiceDialogue(ChoiceDictionary);

                    case JUMP:
                        return TextObject;

                    case ADD:
                        char SecondChar = CurrentString[SECOND_INDEX];
                        String[] CharacterChanges = CurrentString.Substring(CHANGE_INDEX).Split(SPLIT);

                        foreach(String Change in CharacterChanges)
                        {
                            switch(SecondChar)
                            {
                                case FLAG:
                                    MainCharacter.EventFlags.Add(Change);
                                    break;

                                case RELATIONSHIP:
                                    String[] Relationship = Change.Split(ADDSPLIT);
                                    String Character = Relationship[FIRST_INDEX];
                                    int Heart = MainCharacter.Relationships.GetValueOrDefault(Character, 0);

                                    MainCharacter.Relationships.Add(Character, Heart + Int32.Parse(Relationship[SECOND_INDEX]));
                                    break;

                                case STAT:
                                    String[] Stat = Change.Split(ADDSPLIT);
                                    String StatPoint = Stat[FIRST_INDEX];
                                    int Level = MainCharacter.Stats.GetValueOrDefault(StatPoint, 0);

                                    MainCharacter.Stats.Add(StatPoint, Level + Int32.Parse(Stat[SECOND_INDEX]));
                                    break;

                                case NOTEBOOK:
                                    MainCharacter.Journal.Add(Change);
                                    break;
                            }
                        }
                        TextIndex++;
                        break;

                    default:
                        int CharacterIndex = CurrentString.IndexOf(CHARACTER);
                        int AnimationIndex = CurrentString.IndexOf(ANIMATION);
                        int SoundIndex = CurrentString.IndexOf(AUDIO);

                        String Dialogue = CurrentString;
                        String Sound = null;

                        if (CharacterIndex != -1)
                        {
                            Dialogue = CurrentString.Substring(FIRST_INDEX, CharacterIndex);
                            String CharacterName = CurrentString.Substring(CharacterIndex + 1, AnimationIndex - CharacterIndex - 2);
                            String Animation = CurrentString.Substring(AnimationIndex + 1);

                            if (SoundIndex != -1)
                            {
                                Animation = CurrentString.Substring(AnimationIndex + 1, SoundIndex - AnimationIndex - 2);
                                Sound = CurrentString.Substring(SoundIndex + 1);
                            }

                            return new LineOfDialogue(Content, Dialogue, Sound, new CharacterAnimation(Content, CharacterName, Animation));

                        } else if (SoundIndex != -1)
                        {
                            Dialogue = CurrentString.Substring(FIRST_INDEX, SoundIndex);
                            Sound = CurrentString.Substring(SoundIndex + 1);
                        }

                        return new LineOfDialogue(Content, Dialogue, Sound, null);
                }
            }

            return TextObject;
        }
    }
}