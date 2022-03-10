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
    private Dialogue line; //������� �����
    private Sprite lastSprite; //���������� �������������� � ������� ������
    private Story story; //������-���������� Ink
    private float currentPause; //������������ � ������ ������� ����� ����� ���������� ��������
    private Coroutine scrolling; //�������� ������������� ��������� ������
    public bool DialoguePlaying { get; private set; } //��� �� ������ ������?
    private bool lineFinished; //����������� �� ������������ ��������� ����� �� ������?
    private void Awake()
    {
        instance = this;
        DialoguePlaying = false;
        dialoguePanel.SetActive(false);
        lastSprite = null;
        audioPlayer = GetComponent<AudioSource>();
    }
    private void Update()
    {
        if (!DialoguePlaying) return;
        if (Input.GetKeyDown(KeyCode.Space)) {
            StopCoroutine(scrolling);
            if (!lineFinished) { 
                dialogueText.text = line.Text;
                lineFinished = true;
            }
            else ContinueDialogue();
        }
    }

    public static DialogueManager GetInstance()
    {
        return instance;
    }

    public void StartDialogue(TextAsset inkScript, float pause)
    {
        Player.GetPlayer().anim.SetBool("Running", false);
        story = new Story(inkScript.text);
        currentPause = pause;
        DialoguePlaying = true;
        animator.SetBool("IsOpen", true);
        dialoguePanel.SetActive(true);
        ContinueDialogue();
    }

    private void ContinueDialogue()
    {
        if (story.canContinue)
        {
            line = new Dialogue(story.Continue(), currentPause);
            lineFinished = false;
            foreach (string tag in story.currentTags)
            {
                if (tag.StartsWith("sprite"))
                {
                    string spriteID = tag.Substring(7);
                    if (spriteID == "off") dialogueSprite.enabled = false;
                    else
                    {
                        dialogueSprite.enabled = true;
                        lastSprite = Resources.Load<Sprite>(tag.Substring(7));
                        line.SetSprite(lastSprite);
                    }
                }
            }
            dialogueName.text = line.Name;
            scrolling = StartCoroutine(ScrollText());
            dialogueSprite.sprite = line.Sprite ? line.Sprite : lastSprite;
        }
        else
        {
            ExitDialogue();
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
        for(int i = 1; i < line.Text.Length; i++)
        {
            dialogueText.text = line.Text.Substring(0, i);
            if (i == line.Text.Length - 1) lineFinished = true;
            audioPlayer.PlayOneShot(letterSound);
            yield return new WaitForSeconds(line.ScrollPause);
        }
    }

}
