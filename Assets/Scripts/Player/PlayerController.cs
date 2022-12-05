using Interfaces;
using Objects.Player;
using Player;
using Player.Abilities;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public delegate void OnKeyPressed();

    public delegate void OnMove(Vector3 moveVector);

    private const float MirrorVisibilityTolerancy = 0.025F;

    [SerializeField] private Rigidbody rigidBody;
    [SerializeField] private float speed;
    [SerializeField] private float jumpMultiplyer;

    [SerializeField] private ObjectHolder holder;
    [SerializeField] private float activeMirrorsDistance;

    private short _jumpFrameDelay;

    public Mirror NearestMirror { get; private set; }
    public short JumpCount { get; private set; }
    public Rigidbody Rigidbody => rigidBody;
    public float Speed => speed;
    public IMovementStrategy MovementStrategy { get; set; }
    public GameMode GameMode { get; set; }
    public bool IsJumpBlocked { get; set; }

    public OnKeyPressed OnJump { get; set; }
    public OnKeyPressed OnItemGrab { get; set; }
    public OnKeyPressed OnSwitchMode { get; set; }
    public OnKeyPressed OnTimeRewind { get; set; }
    public OnMove OnMoveEvent { get; set; }

    private void Awake()
    {
        PlayerPrefs.SetString("Level", SceneManager.GetActiveScene().name);
        rigidBody.freezeRotation = true;

        IsJumpBlocked = false;
        JumpCount = 0;
        _jumpFrameDelay = Utilities.FramesDelay;

        SetupKeyEvents();
        
        MovementStrategy = new Movement3DStrategy(this);
    }

    private void Update()
    {
        if (Input.GetButtonDown("Jump") && JumpCount < 2 && !IsJumpBlocked)
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
    }

    private void FixedUpdate()
    {
        var horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");
        var movementVector = new Vector3(horizontal, 0, vertical);
        OnMoveEvent?.Invoke(movementVector);
    }

    private void SetupKeyEvents()
    {
        OnJump += () =>
        {
            if (_jumpFrameDelay > 0 || JumpCount > 1)
            {
                return;
            }
            Jump();
            _jumpFrameDelay = Utilities.FramesDelay;
            JumpCount++;
        };

        OnMoveEvent += movementVector =>
        {
            MovementStrategy.Move(movementVector, NearestMirror);
        };
    }

    private void CheckJumpConditions()
    {
        if (_jumpFrameDelay == 0 && Utilities.IsGrounded(transform))
        {
            JumpCount = 0;
        }

        if (_jumpFrameDelay > 0) _jumpFrameDelay--;
    }

    private void Jump()
    {
        rigidBody.AddForce(Vector3.up * jumpMultiplyer, ForceMode.VelocityChange);
    }
    
    private Mirror FindNearestMirror()
    {
        return Utilities.FindNearestMirror(transform, activeMirrorsDistance);
    }

    public bool IsMirrorAccessible(Camera mirrorCam)
    {
        var camTransform = mirrorCam.transform;
        var camPos = camTransform.position;
        return Utilities.IsVisible(transform, mirrorCam, MirrorVisibilityTolerancy)
               || Physics.Raycast(camPos,
                   transform.position - camPos,
                   activeMirrorsDistance,
                   1 << 9);
    }
}