using Interfaces;
using Objects.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    private const float MirrorVisibilityTolerancy = 0.025F;
    
    [SerializeField] private Rigidbody rigidBody;
    [SerializeField] private float speed;
    [SerializeField] private float jumpMultiplyer;
    
    [SerializeField] private Transform reflection;
    [SerializeField] private SkinnedMeshRenderer playerMesh;
    [SerializeField] private Animator anim;
    
    [SerializeField] private ObjectHolder holder;
    [SerializeField] private bool canSwitchModes;
    [SerializeField] private bool canRewindTime;
    [SerializeField] private float activeMirrorsDistance;

    private bool _blockJump;
    private short _jumpCount;
    private short _jumpFrameDelay;


    private static readonly int MidAir = Animator.StringToHash("MidAir");
    private static readonly int Running = Animator.StringToHash("Running");
    private static readonly int JumpAnimationId = Animator.StringToHash("Jump");

    public bool CanSwitchModes => canSwitchModes;
    public bool CanRewindTime => canRewindTime;
    public Mirror NearestMirror { get; private set; }
    public IMovementStrategy MovementStrategy { get; set; }
    public GameModeController GameModeController { get; private set; }


    public OnKeyPressed OnJump { get; set; }
    public OnKeyPressed OnItemGrab { get; set; }
    public OnKeyPressed OnSwitchMode { get; set; }
    public OnKeyPressed OnTimeRewind { get; set; }
    public OnGameModeChangeSuccess OnGameModeChangeSuccessEvent { get; set; }
    public OnGameModeChangeFail OnGameModeChangeFailEvent { get; set; }
    public OnMove OnMoveEvent { get; set; }

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
        
        MovementStrategy = new Movement3DStrategy(this);
        GameModeController = new GameModeController(this);
    }

    private void SetupKeyEvents()
    {
        OnJump += () => {
            Jump();
            DialogueManager.GetInstance().ContinueDialogue();
        };

        OnMoveEvent += (movementVector) =>
        {
            anim.SetBool(Running, movementVector.magnitude >= 0.1f);
            MovementStrategy.Move(movementVector, NearestMirror);
            GameModeController.CheckMirrorBoundaries();
        };
        
        OnSwitchMode += GameModeController.SwitchGameMode;

        OnItemGrab += () =>
        {
            if (GameModeController.GameMode == GameMode.TwoD)
            {
                return;
            }

            holder.ToggleHold();
        };

        OnTimeRewind += () =>
        {
            if (GameModeController.GameMode != GameMode.TwoD || !canRewindTime)
            {
                return;
            }

            var ability = new TimeRewindAbility();
            ability.Use(this);
        };
         
        OnGameModeChangeSuccessEvent += (newGameMode, mirror) => MovementStrategy.StopMovement();
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
        NearestMirror = FindNearestMirror();
        RotateReflection();
    }

            private Mirror FindNearestMirror()
            {
                return Utilities.FindNearestMirror(transform, activeMirrorsDistance);
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
        OnMoveEvent?.Invoke(movementVector);
    }

    private void Jump()
    {
        if (_jumpCount == 0) anim.SetTrigger(JumpAnimationId);
        anim.SetBool(MidAir, true);
        rigidBody.AddForce(Vector3.up * jumpMultiplyer, ForceMode.VelocityChange);
        _jumpFrameDelay = 5;
        _jumpCount++;
    }

            public bool IsMirrorAccessible(Camera mirrorCam)
            {
                Transform camTransform = mirrorCam.transform;
                var camPos = camTransform.position;
                return Utilities.IsVisible(transform, mirrorCam, MirrorVisibilityTolerancy)
                       || Physics.Raycast(camPos,
                           transform.position - camPos,
                           activeMirrorsDistance,
                           1 << 9);
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

    public void SetMeshEnabled(bool state)
    {
        playerMesh.enabled = state;
    }
    
    public delegate void OnKeyPressed();

    public delegate void OnGameModeChangeSuccess(GameMode newGameMode, Mirror mirror);

    public delegate void OnGameModeChangeFail();
    
    public delegate void OnMove(Vector3 moveVector);
}