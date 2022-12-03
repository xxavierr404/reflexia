using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ink.Runtime;

public class DialogueManager : MonoBehaviour
{
    private static DialogueManager instance; //������ ���������

    [SerializeField] private GameObject dialoguePanel; //��������� ���� � ������� ����������
    [SerializeField] private Text dialogueName; //��� ����������
    [SerializeField] private Text dialogueText; //����� ����������
    [SerializeField] private Image dialogueSprite; //������ ����������
    [SerializeField] private Animator animator; //�������� ��������� � ������������ ����
    [SerializeField] private AudioClip letterSound;

    private AudioSource audioPlayer;
    private Dialogue phrase; //������� �����
    private Sprite lastSprite; //���������� �������������� � ������� ������
    private Story story; //������-���������� Ink
    private Coroutine scrolling; //�������� ������������� ��������� ������

    public bool DialoguePlaying { get; private set; } //��� �� ������ ������?
    private bool phraseFinished; //����������� �� ������������ ��������� ����� �� ������?
    private float currentPause; //������������ � ������ ������� ����� ����� ���������� ��������

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
