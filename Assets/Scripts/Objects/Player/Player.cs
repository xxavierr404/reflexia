using UnityEngine;

public class Player : MonoBehaviour
{
    private static Player instance;
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

    private OnKeyPressed OnSpaceEvent { get; set; }
    private OnKeyPressed OnEKeyEvent { get; set; }
    private OnKeyPressed OnQKeyEvent { get; set; }
    public OnGameModeChange OnGameModeChangeSuccess { get; set; }
    public OnGameModeChange OnGameModeChangeFail { get; set; }

    private void Awake()
    {
        instance = this;
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
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && _jumpCount < 2 && !_blockJump &&
            !DialogueManager.GetInstance().DialoguePlaying) Jump();
        if (_jumpFrameDelay == 0 && Utilities.IsGrounded(transform))
        {
            _jumpCount = 0;
            anim.SetBool("MidAir", false);
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
        GameManager.reflectionTeleport = false;
        _jumpFrameDelay = 5;
        _jumpCount++;
    }

    public static Player GetPlayer()
    {
        return instance;
    }

    private delegate void OnKeyPressed();

    public delegate void OnGameModeChange();
}