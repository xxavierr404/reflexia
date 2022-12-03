using System;
using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    
    [SerializeField] private Player player;
    [SerializeField] public float activeMirrorsDistance;
    [SerializeField] private bool allowSwitchingGameMode;
    [SerializeField] private bool allowTimeRewind;
    [SerializeField] private AudioClip mirrorEnterSFX;
    [SerializeField] private AudioClip mirrorExitSFX;
    [SerializeField] private AudioClip failedToSwitch;
    
    private AudioSource audioPlayer;
    private ChromaticAberration chromaticAberration;
    private Vignette vignette;
    private Coroutine focus;

    private void Awake()
    {
        _instance = this;
        PlayerPrefs.SetString("Level", SceneManager.GetActiveScene().name);

        SetupVignette();
        SetupAberration();
        PostProcessManager.instance.QuickVolume(11, 0, vignette, chromaticAberration);

        audioPlayer = GetComponent<AudioSource>();
        
        player.OnGameModeChangeSuccessEvent += (newGameMode, mirror) =>
        {
            StopFocus();
            if (newGameMode == GameMode.TwoD)
            {
                audioPlayer.PlayOneShot(mirrorEnterSFX);
                Ignore3DLayer();
                AnimateCameraToMirror(mirror.transform);
            }
            else
            {
                audioPlayer.PlayOneShot(mirrorExitSFX);
                StopIgnoring3DLayer();
            }
        };

        player.OnGameModeChangeFailEvent += () =>
        {
            audioPlayer.PlayOneShot(failedToSwitch);
        };
    }

    private static void StopIgnoring3DLayer()
    {
        Physics.IgnoreLayerCollision(7, 9, false);
        Camera.main.cullingMask = ~LayerMask.GetMask("PlayerReflection");
        Camera.main.GetComponent<CinemachineBrain>().enabled = true;
    }

    private void SetupAberration()
    {
        chromaticAberration = ScriptableObject.CreateInstance<ChromaticAberration>();
        chromaticAberration.enabled.Override(false);
        chromaticAberration.intensity.Override(1f);
    }

    private void SetupVignette()
    {
        vignette = ScriptableObject.CreateInstance<Vignette>();
        vignette.enabled.Override(false);
        vignette.intensity.Override(0.685f);
        vignette.smoothness.Override(0.8f);
        vignette.roundness.Override(0.2f);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && gameMode == GameMode.TwoD && allowTimeRewind) StartCoroutine(TimeShift());
    }

    public IEnumerator TimeShift()
    {
        timeShiftLocation = reflectionTeleport ? teleportLocation + Vector3.up : player.transform.position;
        var reflectionTexture = mirror.GetComponent<Mirror>().mirrorTexture;
        var freezeTexture = Utilities.ToTexture2D(reflectionTexture);
        var mirrorMaterial = mirror.GetComponent<Mirror>().mirrorMaterial;
        mirrorMaterial.SetTexture("_MainTex", freezeTexture);
        SwitchGameMode();
        vignette = ScriptableObject.CreateInstance<Vignette>();
        StartCoroutine(TimeShiftFX());
        yield return new WaitForSeconds(5);
        mirrorMaterial.SetTexture("_MainTex", reflectionTexture);
        player.transform.position = timeShiftLocation;
    } //Отмотка времени

    private IEnumerator TimeShiftFX()
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

    private void Ignore3DLayer()
    {
        Camera.main.cullingMask = LayerMask.GetMask("Default", "MirrorIgnore");
        Camera.main.GetComponent<CinemachineBrain>().enabled = false;
        Physics.IgnoreLayerCollision(7, 9);
    }

    private void AnimateCameraToMirror(Transform mirror)
    {
        focus = StartCoroutine(Utilities.Focus(Camera.main.transform, transform));
        StartCoroutine(Utilities.LerpRotation(Camera.main.transform, transform, 2.0f));
    }

    private void StopFocus()
    {
        if (focus != null)
        {
            StopCoroutine(focus);
        }
    }
    
    public static GameManager GetInstance()
    {
        return _instance;
    }
    
    public float GetActiveMirrorsDistance()
    {
        return activeMirrorsDistance;
    }

    public bool IsAllowedToSwitchGameModes()
    {
        return allowSwitchingGameMode;
    }
}