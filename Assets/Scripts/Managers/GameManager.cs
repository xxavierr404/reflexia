using System.Collections;
using UnityEngine;
using Cinemachine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    [SerializeField] private float activeMirrorsDistance; //Максимальное расстояние для активации зеркал
    [SerializeField] private GameObject cinemachineBrain; //"Мозг" плагина Cinemachine
    [SerializeField] private bool allowSwitchingGameMode;
    [SerializeField] private bool allowTimeRewind;
    [SerializeField] private AudioClip mirrorEnterSFX;
    [SerializeField] private AudioClip mirrorExitSFX;

    private Player player; //Игрок
    public static GameObject mirror; //Объект ближайшего найденного зеркала
    public static Transform mirrorCam; //Transform камеры самого близкого к игроку на данный момент зеркала
    public static Transform camTransform; //Transform камеры игрока
    public static bool gameMode; //Режим игры, false = 3D, true = 2D
    public static bool reflectionTeleport; //Нужно ли телепортировать игрока при смене режима с 2D на 3D?
    public static Vector3 teleportLocation; //Точка телепортации, если переменная выше истинна

    private Vignette vignette;
    private AudioSource audioPlayer;
    private ChromaticAberration chromaticAberration;
    private Vector3 timeShiftLocation; //Позиция для возврата игрока после отмотки времени
    private bool usingPortableMirror;
    private Coroutine focus;

    private void Awake()
    {
        PlayerPrefs.SetString("Level", SceneManager.GetActiveScene().name);
        usingPortableMirror = false;
        camTransform = Camera.main.transform;
        gameMode = false;
        vignette = ScriptableObject.CreateInstance<Vignette>();
        vignette.enabled.Override(false);
        vignette.intensity.Override(0.685f);
        vignette.smoothness.Override(0.8f);
        vignette.roundness.Override(0.2f);
        chromaticAberration = ScriptableObject.CreateInstance<ChromaticAberration>();
        chromaticAberration.enabled.Override(false);
        chromaticAberration.intensity.Override(1f);
        PostProcessManager.instance.QuickVolume(11, 0, vignette, chromaticAberration);
        audioPlayer = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (!player) player = Player.GetPlayer();
        mirror = Utilities.FindNearestMirror(player.transform, activeMirrorsDistance); //Поиск ближайшего зеркала
        bool isVisible;
        if (mirror)
        {
            mirrorCam = mirror.transform.Find("MirrorCam");
            isVisible = mirror && Utilities.IsVisible(player.reflection, mirrorCam.GetComponent<Camera>(), 0.025f);
        }
        else
        {
            mirrorCam = null;
            isVisible = false;
        }
        if ((Input.GetKeyDown(KeyCode.Q) && isVisible && allowSwitchingGameMode) || (gameMode && !isVisible))
        {
            if (ObjectHolder.Movable != null)
            {
                if (!ObjectHolder.Movable.CompareTag("Mirror")) SwitchGameMode();
            }
            else SwitchGameMode();
        }
        if (Input.GetKeyDown(KeyCode.R) && gameMode && allowTimeRewind) StartCoroutine(TimeShift());
    }
    public void SwitchGameMode()
    {
        if (gameMode == false)
        {
            audioPlayer.PlayOneShot(mirrorEnterSFX);
            Camera.main.cullingMask = LayerMask.GetMask("Default", "MirrorIgnore");
            player.rigidBody.velocity = Vector3.zero;
            Camera.main.GetComponent<CinemachineBrain>().enabled = false;
            mirror.GetComponent<Mirror>().enabled = false;
            player.playerMesh.enabled = false;
            if (mirror.layer == 8)
            {
                mirror.layer = 6;
                usingPortableMirror = true;
            }
            Physics.IgnoreLayerCollision(7, 9);
            focus = StartCoroutine(Utilities.Focus(camTransform, mirror.transform));
            StartCoroutine(Utilities.LerpRotation(camTransform, mirror.transform, 2.0f));
            gameMode = true;
        }
        else
        {
            audioPlayer.PlayOneShot(mirrorExitSFX);
            StopCoroutine(focus);
            Camera.main.cullingMask = ~LayerMask.GetMask("PlayerReflection");
            player.playerMesh.enabled = true;
            mirror.GetComponent<Mirror>().enabled = true;
            if (usingPortableMirror)
            {
                mirror.layer = 8;
                usingPortableMirror = false;
            }
            Physics.IgnoreLayerCollision(7, 9, false);
            if (reflectionTeleport)
            {
                Transform transform = player.transform;
                transform.position = teleportLocation + transform.up;
            }
            Camera.main.GetComponent<CinemachineBrain>().enabled = true;
            reflectionTeleport = false;
            player.blockJump = false;
            gameMode = false;
        }
    } //Смена режима игры

    public IEnumerator TimeShift()
    {
        timeShiftLocation = reflectionTeleport ? teleportLocation + Vector3.up : player.transform.position;
        RenderTexture reflectionTexture = mirror.GetComponent<Mirror>().mirrorTexture;
        Texture2D freezeTexture = Utilities.ToTexture2D(reflectionTexture);
        Material mirrorMaterial = mirror.GetComponent<Mirror>().mirrorMaterial;
        mirrorMaterial.SetTexture("_MainTex", freezeTexture);
        SwitchGameMode();
        vignette = ScriptableObject.CreateInstance<Vignette>();
        StartCoroutine(TimeShiftFX());
        yield return new WaitForSeconds(5);
        mirrorMaterial.SetTexture("_MainTex", reflectionTexture);
        player.transform.position = timeShiftLocation;
    } //Отмотка времени

    public IEnumerator TimeShiftFX()
    {
        vignette.enabled.Override(true);
        chromaticAberration.enabled.Override(true);
        while (vignette.intensity.value > 0f || chromaticAberration.intensity.value > 0f)
        {
            vignette.intensity.value -= 0.02f;
            chromaticAberration.intensity.value -= 0.0137f;
            yield return new WaitForSeconds(0.1f);
        }
        vignette.enabled.Override(false);
        vignette.intensity.Override(0.685f);
        chromaticAberration.enabled.Override(false);
        chromaticAberration.intensity.Override(1f);
    }
}