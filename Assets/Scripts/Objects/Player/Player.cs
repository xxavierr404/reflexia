using Cinemachine;
using UnityEngine;

public class Player : MonoBehaviour
{
    private const float MirrorVisibilityTolerancy = 0.025F;
    
    [SerializeField] private Rigidbody rigidBody;
    [SerializeField] private float jumpMultiplyer; //Множитель высоты прыжков
    [SerializeField] private Transform reflection; //Transform отражения игрока в зеркале
    [SerializeField] private SkinnedMeshRenderer playerMesh; //Mesh модели игрока
    [SerializeField] private float speed; //Скорость передвижения игрока
    [SerializeField] private Animator anim;

    private bool _blockJump; //Флаг для блокировки прыжков
    private short _jumpCount;
    private short _jumpFrameDelay;
    private ObjectHolder _holder;
    private GameMode _gameMode;
    private Mirror _usedMirror;
    private Camera _usedMirrorCamera;
    private bool _reflectionTeleport;
    
    private static readonly int MidAir = Animator.StringToHash("MidAir");
    private static readonly int Running = Animator.StringToHash("Running");

    public OnKeyPressed OnSpaceEvent { get; set; }
    private OnKeyPressed OnEKeyEvent { get; set; }
    private OnKeyPressed OnQKeyEvent { get; set; }
    public OnGameModeChangeSuccess OnGameModeChangeSuccessEvent { get; set; }
    public OnGameModeChangeFail OnGameModeChangeFailEvent { get; set; }

    private void Awake()
    {
        anim = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.freezeRotation = true;
        reflection = transform.Find("PlayerReflection");
        _blockJump = false;
        _jumpCount = 0;
        _jumpFrameDelay = Utilities.FramesDelay;
        OnSpaceEvent += Jump;
        OnSpaceEvent += () => DialogueManager.GetInstance().ContinueDialogue();
        OnEKeyEvent += transform.Find("ObjectHolder").GetComponent<ObjectHolder>().ToggleHold;
        _gameMode = GameMode.ThreeD;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && _jumpCount < 2 && !_blockJump) OnSpaceEvent?.Invoke();
        CheckJumpConditions();
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
        if (GameManager.gameMode == GameMode.TwoD)
            Movement2DManager.Movement2D(movementVector);
        else
            Movement3DManager.Movement3D(movementVector);
    }

    private void Jump()
    {
        if (_jumpCount == 0) anim.SetTrigger("Jump");
        anim.SetBool("MidAir", true);
        rigidBody.AddForce(Vector3.up * jumpMultiplyer, ForceMode.VelocityChange);
        _jumpFrameDelay = 5;
        _jumpCount++;
    }

    public void SwitchGameMode()
    {
        if (!GameManager.GetInstance().IsAllowedToSwitchGameModes())
        {
            return;
        }

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
        var mirror = FindNearestMirror();
        if (!mirror)
        {
            OnGameModeChangeFailEvent?.Invoke();
            return;
        }

        var mirrorCam = mirror.GetComponent<Camera>();
        if (!IsMirrorAccessible(mirrorCam))
        {
            OnGameModeChangeFailEvent?.Invoke();
            return;
        }

        Switch3Dto2D(mirror);
    }

    private bool IsMirrorAccessible(Camera mirrorCam)
    {
        Transform camTransform = mirrorCam.transform;
        var camPos = camTransform.position;
        return Utilities.IsVisible(transform, mirrorCam, MirrorVisibilityTolerancy)
               || Physics.Raycast(camPos,
                   transform.position - camPos,
                   GameManager.GetInstance().GetActiveMirrorsDistance(),
                   1 << 9);
    }

    private void Switch3Dto2D(GameObject mirror)
    {
        _usedMirror = mirror.GetComponent<Mirror>();
        _usedMirror.enabled = false;
        
        rigidBody.velocity = Vector3.zero;
        playerMesh.enabled = false;

        _gameMode = GameMode.TwoD;
        OnGameModeChangeSuccessEvent?.Invoke(_gameMode, mirror);
    }

    private GameObject FindNearestMirror()
    {
        return Utilities.FindNearestMirror(transform, GameManager.GetInstance().GetActiveMirrorsDistance());
    }

    private void Switch2Dto3D()
    {
        playerMesh.enabled = true;
        _usedMirror.enabled = true;
        _reflectionTeleport = false;
        _blockJump = false;
        _gameMode = GameMode.ThreeD;
        OnGameModeChangeSuccessEvent?.Invoke(_gameMode, null);
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
    
    public delegate void OnKeyPressed();

    public delegate void OnGameModeChangeSuccess(GameMode newGameMode, GameObject mirror);

    public delegate void OnGameModeChangeFail();
}