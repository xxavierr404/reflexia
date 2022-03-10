using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dialogue
{
    public string Name { get; private set; }
    public string Text { get; private set; }
    public Sprite Sprite { get; private set; }
    public float ScrollPause { get; private set; }
    public Dialogue(string story, float pause)
    {
        string[] nameAndText = story.Split(':');
        this.Name = nameAndText[0];
        this.Text = nameAndText[1];
        this.ScrollPause = pause;
        this.Sprite = null;
    }
    public void SetSprite(Sprite newSprite)
    {
        Sprite = newSprite;
    }
}