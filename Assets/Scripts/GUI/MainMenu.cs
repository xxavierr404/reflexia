using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private Button loadButton;
    private AudioSource audioPlayer;

    private void Awake()
    {
        audioPlayer = GetComponent<AudioSource>();
        if (!PlayerPrefs.HasKey("Level")) loadButton.gameObject.SetActive(false);
    }

    public void NewGame()
    {
        audioPlayer.PlayOneShot(clickSound);
        PlayerPrefs.SetString("Level", "Intro");
        SceneManager.LoadScene("Intro");
    }

    public void LoadGame()
    {
        audioPlayer.PlayOneShot(clickSound);
        SceneManager.LoadScene(PlayerPrefs.GetString("Level"));
    }

    public void Quit()
    {
        audioPlayer.PlayOneShot(clickSound);
        Application.Quit();
    }
}