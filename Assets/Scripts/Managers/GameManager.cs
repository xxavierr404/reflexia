using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    private static List<Mirror> _mirrorPool;
    
    [SerializeField] private Player player;
    [SerializeField] public float activeMirrorsDistance;
    [SerializeField] private bool allowSwitchingGameMode;
    [SerializeField] private bool allowTimeRewind;
    [SerializeField] private AudioClip mirrorEnterSFX;
    [SerializeField] private AudioClip mirrorExitSFX;
    [SerializeField] private AudioClip failedToSwitch;
    
    private AudioSource _audioPlayer;
    private ChromaticAberration _chromaticAberration;
    private Vignette _vignette;
    private Coroutine _focus;
    private Camera _camera;
    private CinemachineBrain _cinemachineBrain;

    private void Start()
    {
        _camera = Camera.main;
        _cinemachineBrain = _camera.GetComponent<CinemachineBrain>();
    }

    private void Awake()
    {
        _instance = this;
        _mirrorPool = new List<Mirror>();
        PlayerPrefs.SetString("Level", SceneManager.GetActiveScene().name);

        SetupVignette();
        SetupAberration();
        PostProcessManager.instance.QuickVolume(11, 0, _vignette, _chromaticAberration);

        _audioPlayer = GetComponent<AudioSource>();
        
        player.OnGameModeChangeSuccessEvent += (newGameMode, mirror) =>
        {
            StopFocus();
            if (newGameMode == GameMode.TwoD)
            {
                _audioPlayer.PlayOneShot(mirrorEnterSFX);
                Ignore3DLayer();
                AnimateCameraToMirror(mirror.transform);
            }
            else
            {
                _audioPlayer.PlayOneShot(mirrorExitSFX);
                StopIgnoring3DLayer();
            }
        };

        player.OnGameModeChangeFailEvent += () =>
        {
            _audioPlayer.PlayOneShot(failedToSwitch);
        };
    }

    private void StopIgnoring3DLayer()
    {
        Physics.IgnoreLayerCollision(7, 9, false);
        _camera.cullingMask = ~LayerMask.GetMask("PlayerReflection");
        _cinemachineBrain.enabled = true;
    }

    private void SetupAberration()
    {
        _chromaticAberration = ScriptableObject.CreateInstance<ChromaticAberration>();
        _chromaticAberration.enabled.Override(false);
        _chromaticAberration.intensity.Override(1f);
    }

    private void SetupVignette()
    {
        _vignette = ScriptableObject.CreateInstance<Vignette>();
        _vignette.enabled.Override(false);
        _vignette.intensity.Override(0.685f);
        _vignette.smoothness.Override(0.8f);
        _vignette.roundness.Override(0.2f);
    }

    public void StartTimeShiftFX()
    {
        StartCoroutine(TimeShiftFX());
    }
    
    private IEnumerator TimeShiftFX()
    {
        _vignette = ScriptableObject.CreateInstance<Vignette>();
        _vignette.enabled.Override(true);
        _chromaticAberration.enabled.Override(true);
        while (_vignette.intensity.value > 0f || _chromaticAberration.intensity.value > 0f)
        {
            _vignette.intensity.value -= 0.02f;
            _chromaticAberration.intensity.value -= 0.0137f;
            yield return new WaitForSeconds(0.1f);
        }

        _vignette.enabled.Override(false);
        _vignette.intensity.Override(0.685f);
        _chromaticAberration.enabled.Override(false);
        _chromaticAberration.intensity.Override(1f);
    }

    private void Ignore3DLayer()
    {
        _camera.cullingMask = LayerMask.GetMask("Default", "MirrorIgnore");
        _cinemachineBrain.enabled = false;
        Physics.IgnoreLayerCollision(7, 9);
    }

    private void AnimateCameraToMirror(Transform mirror)
    {
        _focus = StartCoroutine(Utilities.Focus(_camera.transform, mirror));
        StartCoroutine(Utilities.LerpRotation(_camera.transform, mirror, 2.0f));
    }

    private void StopFocus()
    {
        if (_focus != null)
        {
            StopCoroutine(_focus);
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

    public bool IsAllowedToUseTimeShift()
    {
        return allowTimeRewind;
    }
    
    public void AddMirrorToPool(Mirror mirror)
    {
        _mirrorPool.Add(mirror);
    }

    public List<Mirror> GetMirrorsPool()
    {
        return _mirrorPool;
    }
}