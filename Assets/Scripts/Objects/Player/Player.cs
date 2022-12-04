using System.Collections;
using Cinemachine;
using Interfaces;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    private const float MirrorVisibilityTolerancy = 0.025F;
    
    [SerializeField] private Rigidbody rigidBody;
    [SerializeField] private float jumpMultiplyer;
    [SerializeField] private Transform reflection;
    [SerializeField] private ObjectHolder holder;
    [SerializeField] private SkinnedMeshRenderer playerMesh;
    [SerializeField] private float speed;
    [SerializeField] private Animator anim;
    [SerializeField] private bool canSwitchModes;
    [SerializeField] private bool canRewindTime;
    [SerializeField] private float activeMirrorsDistance;

    private bool _blockJump;
    private short _jumpCount;
    private short _jumpFrameDelay;
    private GameMode _gameMode;
    private Mirror _nearestMirror;
    private IMovementManager _movementManager;
    
    private static readonly int MidAir = Animator.StringToHash("MidAir");
    private static readonly int Running = Animator.StringToHash("Running");
    private static readonly int JumpAnimationId = Animator.StringToHash("Jump");

    public OnKeyPressed OnJump { get; set; }
    public OnKeyPressed OnItemGrab { get; set; }
    public OnKeyPressed OnSwitchMode { get; set; }
    public OnKeyPressed OnTimeRewind { get; set; }
    public OnGameModeChangeSuccess OnGameModeChangeSuccessEvent { get; set; }
    public OnGameModeChangeFail OnGameModeChangeFailEvent { get; set; }

    private void Awake()
    {
        anim = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.freezeRotation = true;
        PlayerPrefs.SetString("Level", SceneManager.GetActiveScene().name);

        _blockJump = false;
        _jumpCount = 0;
        _jumpFrameDelay = Utilities.FramesDelay;
        
        SetupKeyEvents();

        _gameMode = GameMode.ThreeD;
        _movementManager = new Movement3DManager(this);
    }

    private void SetupKeyEvents()
    {
        OnJump += Jump;
        OnJump += () => DialogueManager.GetInstance().ContinueDialogue();

        OnSwitchMode += SwitchGameMode;

        OnItemGrab += () =>
        {
            if (_gameMode == GameMode.TwoD)
            {
                return;
            }

            holder.ToggleHold();
        };

        OnTimeRewind += () =>
        {
            if (_gameMode != GameMode.TwoD || !canRewindTime)
            {
                return;
            }

            StartCoroutine(TimeShift());
        };
    }

    private void Update()
    {
        if (Input.GetButtonDown("Jump") && _jumpCount < 2 && !_blockJump)
        {
            OnJump?.Invoke();
        }

        if (Input.GetButtonDown("Grab/drop"))
        {
            OnItemGrab?.Invoke();
        }

        if (Input.GetButtonDown("Switch Game Mode"))
        {
            OnSwitchMode?.Invoke();
        }

        if (Input.GetButtonDown("Time Rewind"))
        {
            OnTimeRewind?.Invoke();
        }
        
        CheckJumpConditions();
        _nearestMirror = FindNearestMirror();
        RotateReflection();
    }

    private void CheckJumpConditions()
    {
        if (_jumpFrameDelay == 0 && Utilities.IsGrounded(transform))
        {
            _jumpCount = 0;
            anim.SetBool(MidAir, false);
        }

        if (_jumpFrameDelay > 0) _jumpFrameDelay--;
    }

    private void FixedUpdate()
    {
        var manager = DialogueManager.GetInstance();
        if (manager && manager.DialoguePlaying) return;
        var horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");
        var movementVector = new Vector3(horizontal, 0, vertical);
        anim.SetBool(Running, movementVector.magnitude >= 0.1f);
        _movementManager.Move(movementVector, _nearestMirror);

        if (_gameMode == GameMode.TwoD && !Utilities.IsVisible(transform, _nearestMirror.GetCamera(), 0.025f))
        {
            SwitchGameMode();
        }
    }

    private void Jump()
    {
        if (_jumpCount == 0) anim.SetTrigger(JumpAnimationId);
        anim.SetBool(MidAir, true);
        rigidBody.AddForce(Vector3.up * jumpMultiplyer, ForceMode.VelocityChange);
        _jumpFrameDelay = 5;
        _jumpCount++;
    }

    private void SwitchGameMode()
    {
        if (!canSwitchModes)
        {
            return;
        }

        _movementManager.StopMovement();
        if (_gameMode == GameMode.ThreeD)
        {
            TrySwitch3Dto2D();
        }
        else
        {
            Switch2Dto3D();
        }
    }

    private void TrySwitch3Dto2D()
    {
        if (!_nearestMirror)
        {
            OnGameModeChangeFailEvent?.Invoke();
            return;
        }

        var mirrorCam = _nearestMirror.GetCamera();
        if (!IsMirrorAccessible(mirrorCam))
        {
            OnGameModeChangeFailEvent?.Invoke();
            return;
        }

        Switch3Dto2D(_nearestMirror);
    }

    private bool IsMirrorAccessible(Camera mirrorCam)
    {
        Transform camTransform = mirrorCam.transform;
        var camPos = camTransform.position;
        return Utilities.IsVisible(transform, mirrorCam, MirrorVisibilityTolerancy)
               || Physics.Raycast(camPos,
                   transform.position - camPos,
                   activeMirrorsDistance,
                   1 << 9);
    }

    private void Switch3Dto2D(Mirror mirror)
    {
        _nearestMirror = mirror;
        _nearestMirror.enabled = false;
        
        rigidBody.velocity = Vector3.zero;
        playerMesh.enabled = false;

        _gameMode = GameMode.TwoD;
        OnGameModeChangeSuccessEvent?.Invoke(_gameMode, mirror);
        _movementManager = new Movement2DManager(this);
    }

    private Mirror FindNearestMirror()
    {
        return Utilities.FindNearestMirror(transform, activeMirrorsDistance);
    }

    private void Switch2Dto3D()
    {
        playerMesh.enabled = true;
        _nearestMirror.enabled = true;
        _blockJump = false;
        _gameMode = GameMode.ThreeD;
        OnGameModeChangeSuccessEvent?.Invoke(_gameMode, null);
        _movementManager = new Movement3DManager(this);
    }

    private IEnumerator TimeShift()
    {
        _nearestMirror.Freeze();
        var manager = (Movement2DManager) _movementManager;
        var timeShiftLocation = manager.IsTeleporting() 
            ? manager.GetLastTeleportPosition() + Vector3.up 
            : transform.position;
        SwitchGameMode();
        yield return new WaitForSeconds(5);
        transform.position = timeShiftLocation;
        _nearestMirror.Unfreeze();
    } 

    public Rigidbody GetRigidbody()
    {
        return rigidBody;
    }

    public float GetSpeed()
    {
        return speed;
    }

    public void SetJumpBlock(bool block)
    {
        _blockJump = block;
    }
    
    private void RotateReflection()
    {
        reflection.rotation = FindNearestMirror().GetCamera().transform.rotation;
        reflection.Rotate(0, 0, 180);
    }

    public delegate void OnKeyPressed();

    public delegate void OnGameModeChangeSuccess(GameMode newGameMode, Mirror mirror);

    public delegate void OnGameModeChangeFail();
}