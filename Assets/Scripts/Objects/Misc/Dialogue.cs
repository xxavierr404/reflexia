using UnityEngine;

public class Dialogue
{
    public Dialogue(string story, float pause)
    {
        var nameAndText = story.Split(':');
        Name = nameAndText[0];
        Text = nameAndText[1];
        ScrollPause = pause;
        Sprite = null;
    }

    public Dialogue(string story, float pause, Sprite sprite) : this(story, pause)
    {
        Sprite = sprite;
    }

    public string Name { get; }
    public string Text { get; }
    public Sprite Sprite { get; set; }
    public float ScrollPause { get; }
}