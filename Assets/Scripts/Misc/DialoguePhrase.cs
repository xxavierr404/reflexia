using UnityEngine;

namespace Misc
{
    public class DialoguePhrase
    {
        public string Name { get; }
        public string Text { get; }
        public Sprite Sprite { get; set; }
        public float ScrollPause { get; }

        public DialoguePhrase(string story, float pause)
        {
            var nameAndText = story.Split(':');
            Name = nameAndText[0];
            Text = nameAndText[1];
            ScrollPause = pause;
            Sprite = null;
        }

        public DialoguePhrase(string story, float pause, Sprite sprite) : this(story, pause)
        {
            Sprite = sprite;
        }
    }
}