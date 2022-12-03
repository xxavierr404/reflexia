using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ink.Runtime;

public class DialogueManager : MonoBehaviour
{
    private static DialogueManager instance; //Объект менеджера

    [SerializeField] private GameObject dialoguePanel; //Текстовое окно с фразами персонажей
    [SerializeField] private Text dialogueName; //Имя говорящего
    [SerializeField] private Text dialogueText; //Слова говорящего
    [SerializeField] private Image dialogueSprite; //Спрайт говорящего
    [SerializeField] private Animator animator; //Аниматор появления и исчезновения окна
    [SerializeField] private AudioClip letterSound;

    private AudioSource audioPlayer;
    private Dialogue phrase; //Текущая фраза
    private Sprite lastSprite; //Предыдущий использованный в диалоге спрайт
    private Story story; //Объект-обработчик Ink
    private Coroutine scrolling; //Корутина посимвольного появления текста

    public bool DialoguePlaying { get; private set; } //Идёт ли сейчас диалог?
    private bool phraseFinished; //Закончилось ли посимвольное появление фразы на экране?
    private float currentPause; //Используемая в данном диалоге пауза между появлением символов

    private void Awake()
    {
        instance = this;
        DialoguePlaying = false;
        dialoguePanel.SetActive(false);
        lastSprite = null;
        audioPlayer = GetComponent<AudioSource>();
    }

    public static DialogueManager GetInstance()
    {
        return instance;
    }

    public void StartDialogue(TextAsset inkScript, float pause)
    {
 //       Player.GetPlayer().anim.SetBool("Running", false); // ???
        story = new Story(inkScript.text);
        currentPause = pause;
        DialoguePlaying = true;
        animator.SetBool("IsOpen", true);
        dialoguePanel.SetActive(true);
        ContinueDialogue();
    }

    public void ContinueDialogue()
    {
        if (!DialoguePlaying) return;
        StopCoroutine(scrolling);
        if (!phraseFinished) {
            dialogueText.text = phrase.Text;
            phraseFinished = true;
            return;
        }
        if (story.canContinue)
        {
            LoadPhrase();
            return;
        }
        ExitDialogue();
    }

    private void LoadPhrase()
    {
        phrase = new Dialogue(story.Continue(), currentPause);
        foreach (string tag in story.currentTags)
        {
            if (tag.StartsWith("sprite"))
            {
                LoadSprite(tag);
            }
        }
        dialogueName.text = phrase.Name;
        dialogueSprite.sprite = phrase.Sprite ? phrase.Sprite : lastSprite;
        scrolling = StartCoroutine(ScrollText());
        phraseFinished = false;
    }

    private void LoadSprite(string tag)
    {
        string spriteID = tag.Substring(7);
        if (spriteID == "off") dialogueSprite.enabled = false;
        else
        {
            dialogueSprite.enabled = true;
            lastSprite = Resources.Load<Sprite>(spriteID);
            phrase.Sprite = lastSprite;
        }
    }

    private void ExitDialogue()
    {
        DialoguePlaying = false;
        animator.SetBool("IsOpen", false);
        dialoguePanel.SetActive(false);
        dialogueName.text = "";
        dialogueText.text = "";
        dialogueSprite.sprite = null;
    }

    IEnumerator ScrollText()
    {
        for(int i = 0; i < phrase.Text.Length; i++)
        {
            dialogueText.text = phrase.Text.Substring(0, i);
            if (i == phrase.Text.Length - 1) phraseFinished = true;
            audioPlayer.PlayOneShot(letterSound);
            yield return new WaitForSeconds(phrase.ScrollPause);
        }
    }
}
